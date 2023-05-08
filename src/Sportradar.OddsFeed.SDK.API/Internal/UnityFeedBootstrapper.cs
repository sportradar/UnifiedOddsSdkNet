﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using Common.Logging;
using Dawn;
using Metrics;
using Microsoft.Practices.Unity;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.API.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;
using cashout = Sportradar.OddsFeed.SDK.Messages.REST.cashout;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    internal static class UnityFeedBootstrapper
    {
        private static readonly ILog Log = SdkLoggerFactory.GetLogger(typeof(UnityFeedBootstrapper));

        public static void RegisterBaseTypes(this IUnityContainer container, IOddsFeedConfiguration userConfig)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(userConfig, nameof(userConfig)).NotNull();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            ServicePointManager.DefaultConnectionLimit = OperationManager.MaxConnectionsPerServer;

            Log.Info($"HttpClient using MaxConnectionsPerServer={ServicePointManager.DefaultConnectionLimit}");

            //register common types
            container.RegisterType<HttpClient, HttpClient>(new ContainerControlledLifetimeManager(), new InjectionFactory(
                unityContainer =>
                {
                    var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(userConfig.HttpClientTimeout) };
                    return httpClient;
                }));
            container.RegisterType<HttpClient, HttpClient>("RecoveryHttpClient", new ContainerControlledLifetimeManager(), new InjectionFactory(
                unityContainer =>
                {
                    var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(userConfig.RecoveryHttpClientTimeout) };
                    return httpClient;
                }));
            container.RegisterType<ISdkHttpClient, SdkHttpClient>(new ContainerControlledLifetimeManager(),
                                                                  new InjectionConstructor(
                                                                                           userConfig.AccessToken,
                                                                                           new ResolvedParameter<HttpClient>()));
            container.RegisterType<ISdkHttpClient, SdkHttpClient>("RecoveryHttpClient", new ContainerControlledLifetimeManager(),
                                                                  new InjectionConstructor(
                                                                                           userConfig.AccessToken,
                                                                                           new ResolvedParameter<HttpClient>("RecoveryHttpClient")));

            var seed = (int)DateTime.Now.Ticks;
            var value = SdkInfo.GetRandom(Math.Abs(seed));
            Log.Info($"Initializing sequence generator with MinValue={value}, MaxValue={long.MaxValue}");
            container.RegisterType<ISequenceGenerator, IncrementalSequenceGenerator>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    (long)value,
                    long.MaxValue));

            container.RegisterType<IDeserializer<response>, Deserializer<response>>(new ContainerControlledLifetimeManager());

            container.RegisterType<HttpDataFetcher, HttpDataFetcher>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISdkHttpClient>(),
                    new ResolvedParameter<IDeserializer<response>>(),
                    SdkInfo.RestConnectionFailureLimit,
                    SdkInfo.RestConnectionFailureTimeoutInSec,
                    true));

            container.RegisterType<LogHttpDataFetcher, LogHttpDataFetcher>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISdkHttpClient>(),
                    new ResolvedParameter<ISequenceGenerator>(),
                    new ResolvedParameter<IDeserializer<response>>(),
                    SdkInfo.RestConnectionFailureLimit,
                    SdkInfo.RestConnectionFailureTimeoutInSec));

            var logFetcher = container.Resolve<LogHttpDataFetcher>();
            container.RegisterInstance<IDataFetcher>(logFetcher, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IDataPoster>(logFetcher, new ContainerControlledLifetimeManager());

            container.RegisterType<LogHttpDataFetcher, LogHttpDataFetcher>(
                "RecoveryLogHttpDataFetcher",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISdkHttpClient>("RecoveryHttpClient"),
                    new ResolvedParameter<ISequenceGenerator>(),
                    new ResolvedParameter<IDeserializer<response>>(),
                    SdkInfo.RestConnectionFailureLimit,
                    SdkInfo.RestConnectionFailureTimeoutInSec));

            var recoveryLogFetcher = container.Resolve<LogHttpDataFetcher>("RecoveryLogHttpDataFetcher");
            container.RegisterInstance<IDataFetcher>("RecoveryDataFetcher", recoveryLogFetcher, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IDataPoster>("RecoveryDataPoster", recoveryLogFetcher, new ContainerControlledLifetimeManager());

            container.RegisterType<IDeserializer<bookmaker_details>, Deserializer<bookmaker_details>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDTO>, BookmakerDetailsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<BookmakerDetailsDTO>, BookmakerDetailsProvider>(
                                                                                                 "BookmakerDetailsProvider",
                                                                                                 new ContainerControlledLifetimeManager(),
                                                                                                 new InjectionConstructor(
                                                                                                  "{0}/v1/users/whoami.xml",
                                                                                                  new ResolvedParameter<IDataFetcher>(),
                                                                                                  new ResolvedParameter<IDeserializer<bookmaker_details>>(),
                                                                                                  new ResolvedParameter<ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDTO>>()));

            //container.RegisterInstance(LogProxyFactory.Create<BookmakerDetailsFetcher>(m => m.Name.Contains("Async"), LoggerType.ClientInteraction, true, container.Resolve<IDataProvider<BookmakerDetailsDTO>>()), new ContainerControlledLifetimeManager());

            var bookmakerDetailsProvider = (BookmakerDetailsProvider)container.Resolve<IDataProvider<BookmakerDetailsDTO>>("BookmakerDetailsProvider");
            var config = new OddsFeedConfigurationInternal(userConfig, bookmakerDetailsProvider);

            container.RegisterInstance(config.ExceptionHandlingStrategy, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IOddsFeedConfiguration>(config, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IOddsFeedConfigurationInternal>(config, new ContainerControlledLifetimeManager());
        }

        public static void RegisterTypes(this IUnityContainer container, IGlobalEventDispatcher dispatcher)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(dispatcher, nameof(dispatcher)).NotNull();

            container.RegisterInstance(dispatcher, new ExternallyControlledLifetimeManager());

            container.RegisterType<IConnectionFactory, ConfiguredConnectionFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IChannelFactory, ChannelFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<ConnectionValidator, ConnectionValidator>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISportEntityFactory, SportEntityFactory>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISportDataCache>(),
                    new ResolvedParameter<ISportEventCache>(),
                    new ResolvedParameter<ISportEventStatusCache>(),
                    new ResolvedParameter<ILocalizedNamedValueCache>("MatchStatusCache"),
                    new ResolvedParameter<IProfileCache>(),
                    SdkInfo.SoccerSportUrns
                    ));

            var config = container.Resolve<IOddsFeedConfigurationInternal>();
            container.RegisterType<IEventRecoveryRequestIssuer, RecoveryRequestIssuer>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataPoster>("RecoveryDataPoster"),
                    new ResolvedParameter<ISequenceGenerator>(),
                    config,
                    new ResolvedParameter<IProducerManager>()));

            container.RegisterType<IRecoveryRequestIssuer>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => c.Resolve<IEventRecoveryRequestIssuer>()));

            container.RegisterType<ISportDataProvider, SportDataProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISportEntityFactory>(),
                    new ResolvedParameter<ISportEventCache>(),
                    new ResolvedParameter<ISportEventStatusCache>(),
                    new ResolvedParameter<IProfileCache>(),
                    new ResolvedParameter<ISportDataCache>(),
                    config.Locales,
                    new ResolvedParameter<ExceptionHandlingStrategy>(),
                    new ResolvedParameter<ICacheManager>(),
                    new ResolvedParameter<ILocalizedNamedValueCache>("MatchStatusCache"),
                    new ResolvedParameter<IDataRouterManager>()));

            container.RegisterType<IBookingManager, BookingManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config,
                    new ResolvedParameter<IDataPoster>(),
                    new ResolvedParameter<ICacheManager>()));

            container.RegisterType<IMarketDescriptionManager, MarketDescriptionManager>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(config,
                                             new ResolvedParameter<IMarketCacheProvider>(),
                                             new ResolvedParameter<IMarketDescriptionCache>("InvariantMarketDescriptionsCache"),
                                             new ResolvedParameter<IVariantDescriptionCache>("VariantDescriptionListCache"),
                                             new ResolvedParameter<IMarketDescriptionCache>("VariantMarketDescriptionCache")));

            container.RegisterType<IFeedMessageMapper, FeedMessageMapper>(new ContainerControlledLifetimeManager(),
                                                                          new InjectionConstructor(new ResolvedParameter<ISportEntityFactory>(),
                                                                                                   new ResolvedParameter<INameProviderFactory>(),
                                                                                                   new ResolvedParameter<IMarketMappingProviderFactory
                                                                                                   >(),
                                                                                                   new ResolvedParameter<INamedValuesProvider>(),
                                                                                                   new ResolvedParameter<ExceptionHandlingStrategy>(),
                                                                                                   new ResolvedParameter<IProducerManager>(),
                                                                                                   new ResolvedParameter<IMarketCacheProvider>(),
                                                                                                   new ResolvedParameter<INamedValueCache>("VoidReasonsCache")));

            RegisterNameProviderTypes(container, config.Locales.ToList());
            container.RegisterType<IFeedMessageValidator, FeedMessageValidator>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IMarketCacheProvider>(),
                    config.Locales.First(),
                    new ResolvedParameter<INamedValuesProvider>(),
                    new ResolvedParameter<IProducerManager>()));

            container.RegisterType<IMessageDataExtractor, MessageDataExtractor>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEntityTypeMapper, EntityTypeMapper>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDispatcherStore, DispatcherStore>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISemaphorePool, SemaphorePool>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(500, config.ExceptionHandlingStrategy));

            Func<OddsFeedSession, IEnumerable<string>> func = session => null;

            container.RegisterType<IOddsFeedSession, OddsFeedSession>(
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IMessageReceiver>(),
                    new ResolvedParameter<IFeedMessageProcessor>(),
                    new ResolvedParameter<IFeedMessageMapper>(),
                    new ResolvedParameter<IFeedMessageValidator>(),
                    new ResolvedParameter<IMessageDataExtractor>(),
                    new ResolvedParameter<IDispatcherStore>(),
                    new ResolvedParameter<MessageInterest>(),
                    config.Locales,
                    func));

            container.RegisterType<IOddsFeedSessionBuilder, OddsFeedSessionBuilder>(new TransientLifetimeManager());

            container.RegisterType<ICacheManager, CacheManager>(new ContainerControlledLifetimeManager());

            RegisterNamedValuesProvider(container, config.Locales.ToList(), config);
            RegisterDataRouterManager(container, config);
            RegisterSportEventCache(container, config.Locales.ToList());
            RegisterSportDataCache(container, config.Locales.ToList());
            RegisterCacheMessageProcessor(container);
            RegisterSessionTypes(container, config);
            RegisterProducersProvider(container, config);
            RegisterMarketMappingProviderTypes(container);
            RegisterFeedSystemSession(container);
            RegisterFeedRecoveryManager(container, config);
            RegisterCashOutProbabilitiesProvider(container, config);
            RegisterReplayManager(container, config);
            RegisterCustomBetManager(container, config);
            RegisterEventChangeManager(container, config);
        }

        public static void RegisterAdditionalTypes(this IUnityContainer container)
        {
            var config = container.Resolve<IOddsFeedConfigurationInternal>();
            RegisterSdkStatisticsWriter(container, config);
        }

        private static void RegisterNamedValuesProvider(IUnityContainer container, List<CultureInfo> locales, IOddsFeedConfigurationInternal config)
        {
            // Data provider and cache for void reasons
            container.RegisterType<IDataProvider<EntityList<NamedValueDTO>>, NamedValueDataProvider>(
                "VoidReasonsDataProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/descriptions/void_reasons.xml",
                    new ResolvedParameter<IDataFetcher>(),
                    "void_reason"));

            container.RegisterType<INamedValueCache, NamedValueCache>(
                "VoidReasonsCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataProvider<EntityList<NamedValueDTO>>>("VoidReasonsDataProvider"),
                    new ResolvedParameter<ExceptionHandlingStrategy>(),
                    "VoidReason"));

            // Data provider and cache for bet stop reasons
            container.RegisterType<IDataProvider<EntityList<NamedValueDTO>>, NamedValueDataProvider>(
               "BetStopReasonDataProvider",
               new ContainerControlledLifetimeManager(),
               new InjectionConstructor(
                   config.ApiBaseUri + "/v1/descriptions/betstop_reasons.xml",
                   new ResolvedParameter<IDataFetcher>(),
                   "betstop_reason"));

            container.RegisterType<INamedValueCache, NamedValueCache>(
                "BetStopReasonCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataProvider<EntityList<NamedValueDTO>>>("BetStopReasonDataProvider"),
                    new ResolvedParameter<ExceptionHandlingStrategy>(),
                    "BetstopReason"));

            // Data provider and cache for betting statuses
            container.RegisterType<IDataProvider<EntityList<NamedValueDTO>>, NamedValueDataProvider>(
               "BettingStatusDataProvider",
               new ContainerControlledLifetimeManager(),
               new InjectionConstructor(
                   config.ApiBaseUri + "/v1/descriptions/betting_status.xml",
                   new ResolvedParameter<IDataFetcher>(),
                   "betting_status"));

            container.RegisterType<INamedValueCache, NamedValueCache>(
                "BettingStatusCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataProvider<EntityList<NamedValueDTO>>>("BettingStatusDataProvider"),
                    new ResolvedParameter<ExceptionHandlingStrategy>(),
                    "BettingStatus"));

            // Data provider and cache for match statuses
            container.RegisterType<IDataProvider<EntityList<NamedValueDTO>>, NamedValueDataProvider>(
               "MatchStatusDataProvider",
               new ContainerControlledLifetimeManager(),
               new InjectionConstructor(
                   config.ApiBaseUri + "/v1/descriptions/{0}/match_status.xml",
                   new ResolvedParameter<IDataFetcher>(),
                   "match_status"));

            container.RegisterType<ILocalizedNamedValueCache, LocalizedNamedValueCache>(
                "MatchStatusCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataProvider<EntityList<NamedValueDTO>>>("MatchStatusDataProvider"),
                    locales,
                    new ResolvedParameter<ExceptionHandlingStrategy>(),
                    "MatchStatus"));

            container.RegisterType<INamedValuesProvider, NamedValuesProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<INamedValueCache>("VoidReasonsCache"),
                    new ResolvedParameter<INamedValueCache>("BetStopReasonCache"),
                    new ResolvedParameter<INamedValueCache>("BettingStatusCache"),
                    new ResolvedParameter<ILocalizedNamedValueCache>("MatchStatusCache")));
        }

        private static void RegisterDataRouterManager(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            container.RegisterType<ISdkHttpClient, SdkHttpClientPool>("HttpClientPool",
                                                                      new ContainerControlledLifetimeManager(),
                                                                      new InjectionConstructor(config.AccessToken,
                                                                                               SdkInfo.GetMidValue(config.Locales.Count() * 3, 1, 50),
                                                                                               TimeSpan.FromSeconds(config.HttpClientTimeout)));

            container.RegisterType<ISdkHttpClient, SdkHttpClientPool>("FastHttpClientPool",
                                                                      new ContainerControlledLifetimeManager(),
                                                                      new InjectionConstructor(config.AccessToken,
                                                                                               SdkInfo.GetMidValue(config.Locales.Count() * 7, 1, 100),
                                                                                               OperationManager.FastHttpClientTimeout));

            container.RegisterType<LogHttpDataFetcher, LogHttpDataFetcher>(
                                                                           "LogHttpDataFetcherPool",
                                                                           new TransientLifetimeManager(),
                                                                           new InjectionConstructor(
                                                                                                    new ResolvedParameter<ISdkHttpClient>("HttpClientPool"),
                                                                                                    new ResolvedParameter<ISequenceGenerator>(),
                                                                                                    new ResolvedParameter<IDeserializer<response>>(),
                                                                                                    SdkInfo.RestConnectionFailureLimit,
                                                                                                    SdkInfo.RestConnectionFailureTimeoutInSec));

            container.RegisterType<LogHttpDataFetcher, LogHttpDataFetcher>(
                                                                           "FastLogHttpDataFetcherPool",
                                                                           new TransientLifetimeManager(),
                                                                           new InjectionConstructor(
                                                                                                    new ResolvedParameter<ISdkHttpClient>("FastHttpClientPool"),
                                                                                                    new ResolvedParameter<ISequenceGenerator>(),
                                                                                                    new ResolvedParameter<IDeserializer<response>>(),
                                                                                                    SdkInfo.RestConnectionFailureLimit,
                                                                                                    SdkInfo.RestConnectionFailureTimeoutInSec));

            var nodeIdStr = config.NodeId != 0
                                ? "?node_id=" + config.NodeId
                                : string.Empty;
            // sport event summary provider
            container.RegisterType<IDeserializer<RestMessage>, Deserializer<RestMessage>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<RestMessage, SportEventSummaryDTO>, SportEventSummaryMapperFactory>(new ContainerControlledLifetimeManager());
            var summaryEndpoint = config.Environment == SdkEnvironment.Replay
                                      ? config.ReplayApiBaseUrl + "/sports/{1}/sport_events/{0}/summary.xml" + nodeIdStr
                                      : config.ApiBaseUri + "/v1/sports/{1}/sport_events/{0}/summary.xml";
            container.RegisterType<IDataProvider<SportEventSummaryDTO>, DataProvider<RestMessage, SportEventSummaryDTO>>(
                    "sportEventSummaryProvider",
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        summaryEndpoint,
                        new ResolvedParameter<LogHttpDataFetcher>("FastLogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<RestMessage>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<RestMessage, SportEventSummaryDTO>>()));

            // fixture provider
            container.RegisterType<IDeserializer<fixturesEndpoint>, Deserializer<fixturesEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDTO>, FixtureMapperFactory>(new ContainerControlledLifetimeManager());
            var fixtureEndpoint = config.Environment == SdkEnvironment.Replay
                ? config.ReplayApiBaseUrl + "/sports/{1}/sport_events/{0}/fixture.xml" + nodeIdStr
                : config.ApiBaseUri + "/v1/sports/{1}/sport_events/{0}/fixture.xml";

            container.RegisterType<IDataProvider<FixtureDTO>, DataProvider<fixturesEndpoint, FixtureDTO>>(
                "fixtureEndpointDataProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    fixtureEndpoint,
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<fixturesEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDTO>>()));

            container.RegisterType<IDeserializer<fixturesEndpoint>, Deserializer<fixturesEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDTO>, FixtureMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<FixtureDTO>, DataProvider<fixturesEndpoint, FixtureDTO>>(
                "fixtureChangeFixtureEndpointDataProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(config.Environment == SdkEnvironment.Replay
                    ? config.ReplayApiBaseUrl + "/sports/{1}/sport_events/{0}/fixture.xml" + nodeIdStr
                    : config.ApiBaseUri + "/v1/sports/{1}/sport_events/{0}/fixture_change_fixture.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<fixturesEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDTO>>()));

            // fixture providers for tournamentInfo return data
            container.RegisterType<IDeserializer<tournamentInfoEndpoint>, Deserializer<tournamentInfoEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDTO>, TournamentInfoMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<TournamentInfoDTO>, DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>>(
                "fixtureEndpointForTournamentDataProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    fixtureEndpoint,
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<tournamentInfoEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDTO>>()));

            container.RegisterType<IDeserializer<tournamentInfoEndpoint>, Deserializer<tournamentInfoEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDTO>, TournamentInfoMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<TournamentInfoDTO>, DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>>(
                "fixtureChangeFixtureEndpointForTournamentDataProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.Environment == SdkEnvironment.Replay
                        ? config.ReplayApiBaseUrl + "/sports/{1}/sport_events/{0}/fixture.xml" + nodeIdStr
                        : config.ApiBaseUri + "/v1/sports/{1}/sport_events/{0}/fixture_change_fixture.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<tournamentInfoEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDTO>>()));

            //All available tournaments for all sports
            container.RegisterType<IDeserializer<tournamentsEndpoint>, Deserializer<tournamentsEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<tournamentsEndpoint, EntityList<SportDTO>>, TournamentsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<SportDTO>>, DataProvider<tournamentsEndpoint, EntityList<SportDTO>>>(
                "allTournamentsProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{0}/tournaments.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<tournamentsEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<tournamentsEndpoint, EntityList<SportDTO>>>()));

            //All available sports
            container.RegisterType<IDeserializer<sportsEndpoint>, Deserializer<sportsEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<sportsEndpoint, EntityList<SportDTO>>, SportsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<SportDTO>>, DataProvider<sportsEndpoint, EntityList<SportDTO>>>(
                "allSportsProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{0}/sports.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<sportsEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<sportsEndpoint, EntityList<SportDTO>>>()));

            // date schedule provider
            container.RegisterType<IDeserializer<scheduleEndpoint>, Deserializer<scheduleEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>, DateScheduleMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<SportEventSummaryDTO>>, DateScheduleProvider>(
                "dateScheduleProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{0}/schedules/live/schedule.xml",
                    config.ApiBaseUri + "/v1/sports/{1}/schedules/{0}/schedule.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<scheduleEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>>()));

            // tournament schedule provider
            container.RegisterType<IDeserializer<tournamentSchedule>, Deserializer<tournamentSchedule>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<tournamentSchedule, EntityList<SportEventSummaryDTO>>, TournamentScheduleMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<SportEventSummaryDTO>>, TournamentScheduleProvider>(
                    "tournamentScheduleProvider",
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        config.ApiBaseUri + "/v1/sports/{1}/tournaments/{0}/schedule.xml",
                        new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<tournamentSchedule>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<tournamentSchedule, EntityList<SportEventSummaryDTO>>>()));

            // race schedule for stage tournament provider
            container.RegisterType<IDeserializer<raceScheduleEndpoint>, Deserializer<raceScheduleEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDTO>>, TournamentRaceScheduleMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<SportEventSummaryDTO>>, TournamentRaceScheduleProvider>(
                "tournamentRaceScheduleProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{1}/tournaments/{0}/schedule.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<raceScheduleEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDTO>>>()));

            // player profile provider
            container.RegisterType<IDeserializer<playerProfileEndpoint>, Deserializer<playerProfileEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDTO>, PlayerProfileMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<PlayerProfileDTO>, DataProvider<playerProfileEndpoint, PlayerProfileDTO>>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        config.ApiBaseUri + "/v1/sports/{1}/players/{0}/profile.xml",
                        new ResolvedParameter<LogHttpDataFetcher>("FastLogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<playerProfileEndpoint>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDTO>>()));

            // competitor profile provider
            container.RegisterType<IDeserializer<competitorProfileEndpoint>, Deserializer<competitorProfileEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDTO>, CompetitorProfileMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<CompetitorProfileDTO>, DataProvider<competitorProfileEndpoint, CompetitorProfileDTO>>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        config.ApiBaseUri + "/v1/sports/{1}/competitors/{0}/profile.xml",
                        new ResolvedParameter<LogHttpDataFetcher>("FastLogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<competitorProfileEndpoint>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDTO>>()));

            // simple team profile provider
            container.RegisterType<IDeserializer<simpleTeamProfileEndpoint>, Deserializer<simpleTeamProfileEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDTO>, SimpleTeamProfileMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<SimpleTeamProfileDTO>, DataProvider<simpleTeamProfileEndpoint, SimpleTeamProfileDTO>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{1}/competitors/{0}/profile.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("FastLogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<simpleTeamProfileEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDTO>>()));

            // provider for seasons for a tournament
            container.RegisterType<IDeserializer<tournamentSeasons>, Deserializer<tournamentSeasons>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDTO>, TournamentSeasonsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<TournamentSeasonsDTO>, DataProvider<tournamentSeasons, TournamentSeasonsDTO>>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        config.ApiBaseUri + "/v1/sports/{1}/tournaments/{0}/seasons.xml",
                        new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<tournamentSeasons>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDTO>>()));

            // provider for getting info about ongoing sport event (match timeline)
            container.RegisterType<IDeserializer<matchTimelineEndpoint>, Deserializer<matchTimelineEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDTO>, MatchTimelineMapperFactory>(new ContainerControlledLifetimeManager());
            var timelineEndpoint = config.Environment == SdkEnvironment.Replay
                ? config.ReplayApiBaseUrl + "/sports/{1}/sport_events/{0}/timeline.xml" + nodeIdStr
                : config.ApiBaseUri + "/v1/sports/{1}/sport_events/{0}/timeline.xml";
            container.RegisterType<IDataProvider<MatchTimelineDTO>, DataProvider<matchTimelineEndpoint, MatchTimelineDTO>>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        timelineEndpoint,
                        new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<matchTimelineEndpoint>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDTO>>()));

            // provider for getting info about sport categories
            container.RegisterType<IDeserializer<sportCategoriesEndpoint>, Deserializer<sportCategoriesEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDTO>, SportCategoriesMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<SportCategoriesDTO>, DataProvider<sportCategoriesEndpoint, SportCategoriesDTO>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{1}/sports/{0}/categories.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<sportCategoriesEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDTO>>()));

            // provider for getting info about fixture changes
            container.RegisterType<IDeserializer<fixtureChangesEndpoint>, Deserializer<fixtureChangesEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<fixtureChangesEndpoint, IEnumerable<FixtureChangeDTO>>, FixtureChangesMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<IEnumerable<FixtureChangeDTO>>, DataProvider<fixtureChangesEndpoint, IEnumerable<FixtureChangeDTO>>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{0}/fixtures/changes.xml{1}",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<fixtureChangesEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<fixtureChangesEndpoint, IEnumerable<FixtureChangeDTO>>>()));

            // provider for getting info about result changes
            container.RegisterType<IDeserializer<resultChangesEndpoint>, Deserializer<resultChangesEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<resultChangesEndpoint, IEnumerable<ResultChangeDTO>>, ResultChangesMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<IEnumerable<ResultChangeDTO>>, DataProvider<resultChangesEndpoint, IEnumerable<ResultChangeDTO>>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/sports/{0}/results/changes.xml{1}",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<resultChangesEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<resultChangesEndpoint, IEnumerable<ResultChangeDTO>>>()));

            // invariant market descriptions provider
            container.RegisterType<IDeserializer<market_descriptions>, Deserializer<market_descriptions>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDTO>>, MarketDescriptionsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<MarketDescriptionDTO>>, DataProvider<market_descriptions, EntityList<MarketDescriptionDTO>>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/descriptions/{0}/markets.xml?include_mappings=true",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<market_descriptions>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDTO>>>()));

            // variant market description provider
            container.RegisterType<ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDTO>, MarketDescriptionMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<MarketDescriptionDTO>, DataProvider<market_descriptions, MarketDescriptionDTO>>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(
                        config.ApiBaseUri + "/v1/descriptions/{1}/markets/{0}/variants/{2}?include_mappings=true",
                        new ResolvedParameter<LogHttpDataFetcher>("FastLogHttpDataFetcherPool"),
                        new ResolvedParameter<IDeserializer<market_descriptions>>(),
                        new ResolvedParameter<ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDTO>>()));

            // variant descriptions provider
            container.RegisterType<IDeserializer<variant_descriptions>, Deserializer<variant_descriptions>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDTO>>, VariantDescriptionsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<VariantDescriptionDTO>>, DataProvider<variant_descriptions, EntityList<VariantDescriptionDTO>>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/descriptions/{0}/variants.xml?include_mappings=true",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<variant_descriptions>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDTO>>>()));

            // lottery draw summary provider
            container.RegisterType<IDeserializer<draw_summary>, Deserializer<draw_summary>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<draw_summary, DrawDTO>, DrawSummaryMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<DrawDTO>, DataProvider<draw_summary, DrawDTO>>(
                "drawSummaryProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/wns/{1}/sport_events/{0}/summary.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<draw_summary>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<draw_summary, DrawDTO>>()));

            // lottery draw fixture provider
            container.RegisterType<IDeserializer<draw_fixtures>, Deserializer<draw_fixtures>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<draw_fixtures, DrawDTO>, DrawFixtureMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<DrawDTO>, DataProvider<draw_fixtures, DrawDTO>>(
                "drawFixtureProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/wns/{1}/sport_events/{0}/fixture.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<draw_fixtures>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<draw_fixtures, DrawDTO>>()));

            // lottery schedule provider
            container.RegisterType<IDeserializer<lottery_schedule>, Deserializer<lottery_schedule>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<lottery_schedule, LotteryDTO>, LotteryScheduleMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<LotteryDTO>, DataProvider<lottery_schedule, LotteryDTO>>(
                "lotteryScheduleProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/wns/{1}/lotteries/{0}/schedule.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<lottery_schedule>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<lottery_schedule, LotteryDTO>>()));

            // lottery list provider
            container.RegisterType<IDeserializer<lotteries>, Deserializer<lotteries>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<lotteries, EntityList<LotteryDTO>>, LotteriesMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<LotteryDTO>>, DataProvider<lotteries, EntityList<LotteryDTO>>>(
                "lotteryListProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/wns/{0}/lotteries.xml",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<lotteries>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<lotteries, EntityList<LotteryDTO>>>()));

            // list sport event provider
            container.RegisterType<IDeserializer<scheduleEndpoint>, Deserializer<scheduleEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>, ListSportEventsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<SportEventSummaryDTO>>, ListSportEventsProvider>(
                "listSportEventProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor( // sports/{0}/schedules/pre/schedule.xml?start={1}&limit={2}
                                         config.ApiBaseUri + "/v1/sports/{0}/schedules/pre/schedule.xml?start={1}&limit={2}",
                                         new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                                         new ResolvedParameter<IDeserializer<scheduleEndpoint>>(),
                                         new ResolvedParameter<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>>()));

            // list sport available tournament
            container.RegisterType<IDeserializer<sportTournamentsEndpoint>, Deserializer<sportTournamentsEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<sportTournamentsEndpoint, EntityList<TournamentInfoDTO>>,
                ListSportAvailableTournamentMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<EntityList<TournamentInfoDTO>>, ListSportAvailableTournamentProvider>(
                 "availableSportTournaments",
                 new ContainerControlledLifetimeManager(),
                 new InjectionConstructor( // v1/sports/en/sports/sr:sport:55/tournaments.xml
                                          config.ApiBaseUri + "/v1/sports/{0}/sports/{1}/tournaments.xml",
                                          new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                                          new ResolvedParameter<IDeserializer<sportTournamentsEndpoint>>(),
                                          new ResolvedParameter<ISingleTypeMapperFactory<sportTournamentsEndpoint, EntityList<TournamentInfoDTO>>>()));

            // get the stage period summary (lap statistics - for formula 1)
            container.RegisterType<IDeserializer<stagePeriodEndpoint>, Deserializer<stagePeriodEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDTO>, PeriodSummaryMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<PeriodSummaryDTO>, DataProvider<stagePeriodEndpoint, PeriodSummaryDTO>>(
                "stagePeriodSummaryProvider",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor( // v1/sports/en/sport_events/sr:stage:{id}/period_summary.xml?competitors=sr:competitor:{id}&competitors=sr:competitor:{id}&periods=2&periods=3&periods=4
                    config.ApiBaseUri + "/v1/sports/{0}/sport_events/{1}/period_summary.xml{2}",
                    new ResolvedParameter<LogHttpDataFetcher>("LogHttpDataFetcherPool"),
                    new ResolvedParameter<IDeserializer<stagePeriodEndpoint>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDTO>>()));

            container.RegisterType<IDataRouterManager, DataRouterManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ICacheManager>(),
                    new ResolvedParameter<IProducerManager>(),
                    new ResolvedParameter<ExceptionHandlingStrategy>(),
                    config.DefaultLocale,
                    new ResolvedParameter<IDataProvider<SportEventSummaryDTO>>("sportEventSummaryProvider"),
                    new ResolvedParameter<IDataProvider<FixtureDTO>>("fixtureEndpointDataProvider"),
                    new ResolvedParameter<IDataProvider<FixtureDTO>>("fixtureChangeFixtureEndpointDataProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<SportDTO>>>("allTournamentsProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<SportDTO>>>("allSportsProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<SportEventSummaryDTO>>>("dateScheduleProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<SportEventSummaryDTO>>>("tournamentScheduleProvider"),
                    new ResolvedParameter<IDataProvider<PlayerProfileDTO>>(),
                    new ResolvedParameter<IDataProvider<CompetitorProfileDTO>>(),
                    new ResolvedParameter<IDataProvider<SimpleTeamProfileDTO>>(),
                    new ResolvedParameter<IDataProvider<TournamentSeasonsDTO>>(),
                    new ResolvedParameter<IDataProvider<MatchTimelineDTO>>(),
                    new ResolvedParameter<IDataProvider<SportCategoriesDTO>>(),
                    new ResolvedParameter<IDataProvider<EntityList<MarketDescriptionDTO>>>(),
                    new ResolvedParameter<IDataProvider<MarketDescriptionDTO>>(),
                    new ResolvedParameter<IDataProvider<EntityList<VariantDescriptionDTO>>>(),
                    new ResolvedParameter<IDataProvider<DrawDTO>>("drawSummaryProvider"),
                    new ResolvedParameter<IDataProvider<DrawDTO>>("drawFixtureProvider"),
                    new ResolvedParameter<IDataProvider<LotteryDTO>>("lotteryScheduleProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<LotteryDTO>>>("lotteryListProvider"),
                    new ResolvedParameter<IDataProvider<AvailableSelectionsDto>>(),
                    new ResolvedParameter<ICalculateProbabilityProvider>(),
                    new ResolvedParameter<ICalculateProbabilityFilteredProvider>(),
                    new ResolvedParameter<IDataProvider<IEnumerable<FixtureChangeDTO>>>(),
                    new ResolvedParameter<IDataProvider<IEnumerable<ResultChangeDTO>>>(),
                    new ResolvedParameter<IDataProvider<EntityList<SportEventSummaryDTO>>>("listSportEventProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<TournamentInfoDTO>>>("availableSportTournaments"),
                    new ResolvedParameter<IDataProvider<TournamentInfoDTO>>("fixtureEndpointForTournamentDataProvider"),
                    new ResolvedParameter<IDataProvider<TournamentInfoDTO>>("fixtureChangeFixtureEndpointForTournamentDataProvider"),
                    new ResolvedParameter<IDataProvider<PeriodSummaryDTO>>("stagePeriodSummaryProvider"),
                    new ResolvedParameter<IDataProvider<EntityList<SportEventSummaryDTO>>>("tournamentRaceScheduleProvider")));
        }

        private static void RegisterSessionTypes(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            var connectionTimer = new SdkTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
            var maxTimeBetweenMessages = TimeSpan.FromSeconds(180);

            container.RegisterType<IDeserializer<FeedMessage>, Deserializer<FeedMessage>>(new HierarchicalLifetimeManager());
            container.RegisterType<IRoutingKeyParser, RegexRoutingKeyParser>(new HierarchicalLifetimeManager());
            container.RegisterType<IRabbitMqChannel, RabbitMqChannel>(new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IChannelFactory>(),
                    connectionTimer,
                    maxTimeBetweenMessages,
                    config.AccessToken
                ));
            container.RegisterType<IMessageReceiver, RabbitMqMessageReceiver>(
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IRabbitMqChannel>(),
                    new ResolvedParameter<IDeserializer<FeedMessage>>(),
                    new ResolvedParameter<IRoutingKeyParser>(),
                    new ResolvedParameter<IProducerManager>(),
                    config.Environment == SdkEnvironment.Replay));

            //IRabbitMqChannel channel, IDeserializer<FeedMessage> deserializer, IRoutingKeyParser keyParser, IProducerManager producerManager, bool usedReplay

            container.RegisterType<IFeedMessageProcessor>(
                "SessionMessageManager",
                new HierarchicalLifetimeManager(),
                new InjectionFactory(c => c.Resolve<IFeedRecoveryManager>().CreateSessionMessageManager()));

            container.RegisterType<IFeedMessageProcessor>(
                new HierarchicalLifetimeManager(),
                new InjectionFactory(c => new CompositeMessageProcessor(c.ResolveAll<IFeedMessageProcessor>().ToList())));
        }

        private static void RegisterCacheMessageProcessor(IUnityContainer container)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.RegisterInstance(
                "FixtureChangeCache_Cache",
                new MemoryCache("fixtureCacheCache", new NameValueCollection { { "CacheMemoryLimitMegabytes", "16" } }),
                new ContainerControlledLifetimeManager());

            container.RegisterInstance(
                "FixtureChangeCacheItemPolicy",
                new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(10) },
                new ContainerControlledLifetimeManager());

            container.RegisterType<IFeedMessageHandler, FeedMessageHandler>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("FixtureChangeCache_Cache"),
                    new ResolvedParameter<CacheItemPolicy>("FixtureChangeCacheItemPolicy")));

            container.RegisterType<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO>, SportEventStatusMapperFactory>(
                new ContainerControlledLifetimeManager());

            container.RegisterInstance(
                "EventStatusCache_Cache",
                new MemoryCache("eventStatusCache"),
                new ContainerControlledLifetimeManager());

            container.RegisterInstance("IgnoreEventsTimelineCache_Cache",
                                       new MemoryCache("ignoreEventsTimelineCache"),
                                       new ContainerControlledLifetimeManager());

            container.RegisterType<ISportEventStatusCache, SportEventStatusCache>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("EventStatusCache_Cache"),
                    new ResolvedParameter<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO>>(),
                    new ResolvedParameter<ISportEventCache>(),
                    new ResolvedParameter<ICacheManager>(),
                    new ResolvedParameter<MemoryCache>("IgnoreEventsTimelineCache_Cache")));

            container.RegisterType<IFeedMessageProcessor, CacheMessageProcessor>(
                "CacheMessageProcessor",
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO>>(),
                    new ResolvedParameter<ISportEventCache>(),
                    new ResolvedParameter<ICacheManager>(),
                    new ResolvedParameter<IFeedMessageHandler>(),
                    new ResolvedParameter<ISportEventStatusCache>(),
                    new ResolvedParameter<IProducerManager>()));

            container.RegisterType<CacheMessageProcessor>(
                new HierarchicalLifetimeManager(),
                new InjectionFactory(c => c.Resolve<IFeedMessageProcessor>("CacheMessageProcessor")));
        }

        private static void RegisterSportEventCache(IUnityContainer container, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.RegisterInstance(
                "SportEventCache_Cache",
                new MemoryCache("sportEventCache"),
                new ContainerControlledLifetimeManager());

            container.RegisterInstance(
                "SportEventCache_FixtureTimestampCache",
                new MemoryCache("sportEventFixtureTimestampCache"),
                new ContainerControlledLifetimeManager());

            container.RegisterType<ITimer, SdkTimer>(
                "SportEventCacheTimer",
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromHours(24)));

            // SportEventCacheItemFactory
            container.RegisterType<ISportEventCacheItemFactory, SportEventCacheItemFactory>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<ISemaphorePool>(),
                    cultures.First(),
                    new ResolvedParameter<MemoryCache>("SportEventCache_FixtureTimestampCache")));

            container.RegisterType<IDeserializer<scheduleEndpoint>, Deserializer<scheduleEndpoint>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>, SportEventsScheduleMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISportEventCache, SportEventCache>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("SportEventCache_Cache"),
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<ISportEventCacheItemFactory>(),
                    new ResolvedParameter<ITimer>("SportEventCacheTimer"),
                    cultures,
                    new ResolvedParameter<ICacheManager>()));
        }

        private static void RegisterSportDataCache(IUnityContainer container, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.RegisterType<ITimer, SdkTimer>(
                "SportDataCacheTimer",
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromHours(12)));

            container.RegisterType<ISportDataCache, SportDataCache>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<ITimer>("SportDataCacheTimer"),
                    cultures,
                    new ResolvedParameter<ISportEventCache>(),
                    new ResolvedParameter<ICacheManager>()));
        }

        private static void RegisterNameProviderTypes(IUnityContainer container, IEnumerable<CultureInfo> cultures)
        {
            // Cache for invariant markets
            container.RegisterInstance(
                "InvariantMarketDescriptionsCache_Cache",
                new MemoryCache("invariantMarketsDescriptionsCache"),
                new ContainerControlledLifetimeManager());

            // Timer for invariant markets
            container.RegisterType<ITimer, SdkTimer>("InvariantMarketCacheTimer",
                                                     new HierarchicalLifetimeManager(),
                                                     new InjectionConstructor(TimeSpan.FromSeconds(5),
                                                                              TimeSpan.FromHours(6)));

            container.RegisterType<IMappingValidatorFactory, MappingValidatorFactory>(new ContainerControlledLifetimeManager());

            // Invariant market cache
            container.RegisterType<IMarketDescriptionCache, InvariantMarketDescriptionCache>(
                "InvariantMarketDescriptionsCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("InvariantMarketDescriptionsCache_Cache"),
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<IMappingValidatorFactory>(),
                    new ResolvedParameter<ITimer>("InvariantMarketCacheTimer"),
                    cultures,
                    new ResolvedParameter<ICacheManager>()));

            // Cache for variant markets
            container.RegisterInstance(
                "VariantMarketDescriptionCache_Cache",
                new MemoryCache("variantMarketsDescriptionsCache"),
                new ContainerControlledLifetimeManager());

            // Variant market cache
            container.RegisterType<IMarketDescriptionCache, VariantMarketDescriptionCache>(
                "VariantMarketDescriptionCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("VariantMarketDescriptionCache_Cache"),
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<IMappingValidatorFactory>(),
                    new ResolvedParameter<ICacheManager>()));

            // Cache for variant descriptions
            container.RegisterInstance(
                "VariantDescriptionListCache_Cache",
                new MemoryCache("variantDescriptionListCache"),
                new ContainerControlledLifetimeManager());

            // Timer for variant descriptions
            container.RegisterType<ITimer, SdkTimer>("VariantDescriptionListCacheTimer",
                                                     new HierarchicalLifetimeManager(),
                                                     new InjectionConstructor(TimeSpan.FromSeconds(5),
                                                                              TimeSpan.FromHours(6)));

            container.RegisterType<IMappingValidatorFactory, MappingValidatorFactory>(new ContainerControlledLifetimeManager());

            // Variant descriptions cache
            container.RegisterType<IVariantDescriptionCache, VariantDescriptionListCache>(
                "VariantDescriptionListCache",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("VariantDescriptionListCache_Cache"),
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<IMappingValidatorFactory>(),
                    new ResolvedParameter<ITimer>("VariantDescriptionListCacheTimer"),
                    cultures,
                    new ResolvedParameter<ICacheManager>()));

            // Market cache selector
            container.RegisterType<IMarketCacheProvider, MarketCacheProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IMarketDescriptionCache>("InvariantMarketDescriptionsCache"),
                    new ResolvedParameter<IMarketDescriptionCache>("VariantMarketDescriptionCache"),
                    new ResolvedParameter<IVariantDescriptionCache>("VariantDescriptionListCache")));

            // Cache for player and competitor profiles
            container.RegisterInstance(
                "ProfileCache_Cache",
                new MemoryCache("profileCache"),
                new ContainerControlledLifetimeManager());

            container.RegisterType<IProfileCache, ProfileCache>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("ProfileCache_Cache"),
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<ICacheManager>(),
                    new ResolvedParameter<ISportEventCache>()));

            container.RegisterType<IOperandFactory, OperandFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<INameExpressionFactory, NameExpressionFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<INameProviderFactory, NameProviderFactory>(new ContainerControlledLifetimeManager());
        }

        private static void RegisterMarketMappingProviderTypes(IUnityContainer container)
        {
            container.RegisterType<IMarketMappingProviderFactory, MarketMappingProviderFactory>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IMarketCacheProvider>(),
                    new ResolvedParameter<ISportEventStatusCache>(),
                    new ResolvedParameter<ExceptionHandlingStrategy>()));
        }

        private static void RegisterCashOutProbabilitiesProvider(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            container.RegisterType<IDeserializer<cashout>, Deserializer<cashout>>(
                new ContainerControlledLifetimeManager());

            container.RegisterType<IDataProvider<cashout>, NonMappingDataProvider<cashout>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/probabilities/{0}",
                    new ResolvedParameter<IDataFetcher>(),
                    new ResolvedParameter<IDeserializer<cashout>>()));

            container.RegisterType<ICashOutProbabilitiesProvider, CashOutProbabilitiesProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataProvider<cashout>>(),
                    new ResolvedParameter<IFeedMessageMapper>(),
                    config.Locales,
                    config.ExceptionHandlingStrategy));
        }

        private static void RegisterProducersProvider(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            container.RegisterType<IDeserializer<producers>, Deserializer<producers>>(
                new ContainerControlledLifetimeManager());

            container.RegisterType<IDataProvider<producers>, NonMappingDataProvider<producers>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/descriptions/producers.xml",
                    new ResolvedParameter<IDataFetcher>(),
                    new ResolvedParameter<IDeserializer<producers>>()));

            container.RegisterType<IProducersProvider, ProducersProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataProvider<producers>>(),
                    config));

            container.RegisterType<IProducerManager, ProducerManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IProducersProvider>(),
                    config));
        }

        private static void RegisterSdkStatisticsWriter(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            var statusProviders = new List<IHealthStatusProvider>
            {
                container.Resolve<LogHttpDataFetcher>(),
                container.Resolve<SportEventCache>(),
                container.Resolve<SportDataCache>(),
                container.Resolve<InvariantMarketDescriptionCache>("InvariantMarketDescriptionsCache"),
                container.Resolve<VariantMarketDescriptionCache>("VariantMarketDescriptionCache"),
                container.Resolve<VariantDescriptionListCache>("VariantDescriptionListCache"),
                container.Resolve<ProfileCache>(),
                container.Resolve<LocalizedNamedValueCache>("MatchStatusCache"),
                container.Resolve<SportEventStatusCache>()
            };

            container.RegisterType<MetricsReporter, MetricsReporter>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    MetricsReportPrintMode.Full,
                    2,
                    true));
            var metricReporter = container.Resolve<MetricsReporter>();

            Metric.Config.WithAllCounters().WithReporting(rep => rep.WithReport(metricReporter, TimeSpan.FromSeconds(config.StatisticsTimeout)));

            container.RegisterInstance(metricReporter, new ContainerControlledLifetimeManager());

            foreach (var sp in statusProviders)
            {
                sp.RegisterHealthCheck();
            }
        }

        private static void RegisterFeedRecoveryManager(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            container.RegisterType<IProducerRecoveryManagerFactory, ProducerRecoveryManagerFactory>(new HierarchicalLifetimeManager());

            container.RegisterType<ITimer, SdkTimer>(
                "FeedRecoveryManagerTimer",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(60)));

            container.RegisterType<IFeedRecoveryManager, FeedRecoveryManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IProducerRecoveryManagerFactory>(),
                    config,
                    new ResolvedParameter<ITimer>("FeedRecoveryManagerTimer"),
                    new ResolvedParameter<IProducerManager>(),
                    new ResolvedParameter<FeedSystemSession>()));
        }

        private static void RegisterReplayManager(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            var sdkHtpClient = container.Resolve<ISdkHttpClient>();

            container.RegisterType<IDataRestful, HttpDataRestful>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    sdkHtpClient,
                    new ResolvedParameter<IDeserializer<response>>(),
                    SdkInfo.RestConnectionFailureLimit,
                    SdkInfo.RestConnectionFailureTimeoutInSec));
            object[] argsRest =
            {
                sdkHtpClient,
                new Deserializer<response>(),
                SdkInfo.RestConnectionFailureLimit,
                SdkInfo.RestConnectionFailureTimeoutInSec
            };

            container.RegisterInstance<IDataRestful>(LogProxyFactory.Create<HttpDataRestful>(argsRest, m => m.Name.Contains("Async"), LoggerType.RestTraffic),
                new ContainerControlledLifetimeManager());

            container.RegisterType<IReplayManagerV1, ReplayManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ReplayApiBaseUrl,
                    new ResolvedParameter<IDataRestful>(),
                    config.NodeId));

            object[] args =
            {
                config.ReplayApiBaseUrl,
                container.Resolve<IDataRestful>(),
                config.NodeId
            };
            container.RegisterInstance<IReplayManagerV1>(LogProxyFactory.Create<ReplayManager>(args, m => m.Name.Contains("e"), LoggerType.ClientInteraction),
                new ContainerControlledLifetimeManager());
        }

        private static void RegisterFeedSystemSession(IUnityContainer container)
        {
            container.RegisterType<FeedSystemSession, FeedSystemSession>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IGlobalEventDispatcher>(),
                    new ResolvedParameter<IMessageReceiver>(),
                    new ResolvedParameter<IFeedMessageMapper>(),
                    new ResolvedParameter<IFeedMessageValidator>(),
                    new ResolvedParameter<IMessageDataExtractor>()));
        }

        private static void RegisterCustomBetManager(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            container.RegisterType<ICustomBetSelectionBuilder, CustomBetSelectionBuilder>(new ContainerControlledLifetimeManager());

            container.RegisterType<IDeserializer<AvailableSelectionsType>, Deserializer<AvailableSelectionsType>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<AvailableSelectionsType, AvailableSelectionsDto>, AvailableSelectionsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<AvailableSelectionsDto>, DataProvider<AvailableSelectionsType, AvailableSelectionsDto>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/custombet/{0}/available_selections",
                    new ResolvedParameter<LogHttpDataFetcher>(),
                    new ResolvedParameter<IDeserializer<AvailableSelectionsType>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<AvailableSelectionsType, AvailableSelectionsDto>>()));

            container.RegisterType<IDeserializer<CalculationResponseType>, Deserializer<CalculationResponseType>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>, CalculationMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICalculateProbabilityProvider, CalculateProbabilityProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    config.ApiBaseUri + "/v1/custombet/calculate",
                    new ResolvedParameter<LogHttpDataFetcher>(),
                    new ResolvedParameter<IDeserializer<CalculationResponseType>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>>()));

            container.RegisterType<IDeserializer<FilteredCalculationResponseType>, Deserializer<FilteredCalculationResponseType>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto>, CalculationFilteredMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICalculateProbabilityFilteredProvider, CalculateProbabilityFilteredProvider>(
                                                                                                                new ContainerControlledLifetimeManager(),
                                                                                                                new InjectionConstructor(
                                                                                                                 config.ApiBaseUri + "/v1/custombet/calculate-filter",
                                                                                                                 new ResolvedParameter<LogHttpDataFetcher>(),
                                                                                                                 new ResolvedParameter<IDeserializer<FilteredCalculationResponseType>>(),
                                                                                                                 new ResolvedParameter<ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto>>()));


            container.RegisterType<ICustomBetManager, CustomBetManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IDataRouterManager>(),
                    new ResolvedParameter<ICustomBetSelectionBuilder>(),
                    config.ExceptionHandlingStrategy));
        }

        private static void RegisterEventChangeManager(IUnityContainer container, IOddsFeedConfigurationInternal config)
        {
            container.RegisterType<IEventChangeManager, EventChangeManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    TimeSpan.FromMinutes(60),
                    TimeSpan.FromMinutes(60),
                    new ResolvedParameter<ISportDataProvider>(),
                    new ResolvedParameter<ISportEventCache>(),
                    config));
        }
    }
}
