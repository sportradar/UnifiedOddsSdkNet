﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Provides access to sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    [Log(LoggerType.ClientInteraction)]
    internal class SportDataProvider : ISportDataProviderV7
    {
        private static readonly ILog Log = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(SportDataProvider));

        /// <summary>
        /// A <see cref="ISportEntityFactory"/> used to construct <see cref="ITournament"/> instances
        /// </summary>
        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// A <see cref="ISportEventCache"/> used to retrieve schedules for sport events
        /// </summary>
        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// A <see cref="ISportEventStatusCache"/> used to retrieve status for sport event
        /// </summary>
        private readonly ISportEventStatusCache _sportEventStatusCache;

        /// <summary>
        /// The profile cache used to retrieve competitor or player profile
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// The sport data cache used to retrieve sport data
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// A <see cref="IList{CultureInfo}"/> specified as default cultures (from configuration)
        /// </summary>
        private readonly IReadOnlyCollection<CultureInfo> _defaultCultures;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying enum member specifying how instances provided by the current provider will handle exceptions
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        /// <summary>
        /// The cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// The match status cache
        /// </summary>
        private readonly ILocalizedNamedValueCache _matchStatusCache;

        /// <summary>
        /// The data router manager
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDataProvider"/> class
        /// </summary>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to construct <see cref="ITournament"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to retrieve schedules for sport events</param>
        /// <param name="sportEventStatusCache">A <see cref="ISportEventStatusCache"/> used to retrieve status for sport event</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> ued to retrieve competitor or player profile</param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> ued to retrieve sport data</param>
        /// <param name="defaultCultures"> A <see cref="IList{CultureInfo}"/> specified as default cultures (from configuration)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying enum member specifying how instances provided by the current provider will handle exceptions</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="matchStatusCache">A <see cref="ILocalizedNamedValueCache"/> used to retrieve match statuses</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to invoke API requests</param>
        public SportDataProvider(ISportEntityFactory sportEntityFactory,
                                 ISportEventCache sportEventCache,
                                 ISportEventStatusCache sportEventStatusCache,
                                 IProfileCache profileCache,
                                 ISportDataCache sportDataCache,
                                 IEnumerable<CultureInfo> defaultCultures,
                                 ExceptionHandlingStrategy exceptionStrategy,
                                 ICacheManager cacheManager,
                                 ILocalizedNamedValueCache matchStatusCache,
                                 IDataRouterManager dataRouterManager)
        {
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(defaultCultures, nameof(defaultCultures)).NotNull();//.NotEmpty();
            if (!defaultCultures.Any())
                throw new ArgumentOutOfRangeException(nameof(defaultCultures));

            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();
            Guard.Argument(matchStatusCache, nameof(matchStatusCache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _sportEventCache = sportEventCache;
            _sportEventStatusCache = sportEventStatusCache;
            _profileCache = profileCache;
            _sportDataCache = sportDataCache;
            _defaultCultures = defaultCultures as IReadOnlyCollection<CultureInfo>;
            _exceptionStrategy = exceptionStrategy;
            _cacheManager = cacheManager;
            _matchStatusCache = matchStatusCache;
            _dataRouterManager = dataRouterManager;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ISport}"/> representing all available sports in language specified by the <code>culture</code>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] {culture};
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetSportsAsync: [Cultures={s}].");
            var result = await _sportEntityFactory.BuildSportsAsync(cs, _exceptionStrategy).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISport"/> instance representing the sport specified by it's id in the language specified by <code>culture</code>, or a null reference if sport with specified id does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> identifying the sport to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{ISport}"/> representing the async operation</returns>
        public async Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetSportsAsync: [Id={id}, Cultures={s}].");
            return await _sportEntityFactory.BuildSportAsync(id, cs,_exceptionStrategy).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing currently live sport events in the language specified by <code>culture</code>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ICompetition>> GetLiveSportEventsAsync(CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetLiveSportEventsAsync: [Cultures={s}].");

            var tasks = cs.Select(c => _sportEventCache.GetEventIdsAsync((DateTime?) null, c)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);

            var ids = tasks.First().Result;
            return ids.Select(item => _sportEntityFactory.BuildSportEvent<ICompetition>(item.Item1,
                                                                                        item.Item2,
                                                                                        culture == null ? _defaultCultures : new[] {culture},
                                                                                        _exceptionStrategy))
                      .ToList();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing sport events scheduled for date specified by <code>date</code> in language specified by <code>culture</code>
        /// </summary>
        /// <param name="date">A <see cref="DateTime"/> specifying the day for which to retrieve the schedule</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ICompetition>> GetSportEventsByDateAsync(DateTime date, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetSportEventsByDateAsync: [Date={date}, Cultures={s}].");

            var tasks = cs.Select(c => _sportEventCache.GetEventIdsAsync(date, c)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);

            var ids = tasks.First().Result;
            return ids.Select(item => _sportEntityFactory.BuildSportEvent<ICompetition>(item.Item1,
                                                                                        item.Item2,
                                                                                        culture == null ? _defaultCultures : new[] {culture},
                                                                                        _exceptionStrategy))
                      .ToList();
        }

        /// <summary>
        /// Gets a <see cref="ILongTermEvent"/> representing the specified tournament in language specified by <code>culture</code> or a null reference if the tournament with
        /// specified <code>id</code> does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the tournament to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ILongTermEvent"/> representing the specified tournament or a null reference if requested tournament does not exist</returns>
        public ILongTermEvent GetTournament(URN id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetTournament: [Id={id}, Cultures={s}].");

            var result = _sportEntityFactory.BuildSportEvent<ILongTermEvent>(id,
                                                                             null,
                                                                             culture == null
                                                                                 ? _defaultCultures
                                                                                 : new[] {culture},
                                                                             _exceptionStrategy);

            Log.Info($"GetTournament returned: {result?.Id}.");
            return result;
        }

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <code>culture</code> or a null reference if the sport event with
        /// specified <code>id</code> does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the sport event to retrieve</param>
        /// <param name="sportId">A <see cref="URN"/> of the sport this event belongs to</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        public ICompetition GetCompetition(URN id, URN sportId, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetCompetition: [Id={id}, SportId={sportId}, Cultures={s}].");

            var result = _sportEntityFactory.BuildSportEvent<ICompetition>(id,
                                                                           sportId,
                                                                           culture == null
                                                                               ? _defaultCultures
                                                                               : new[] {culture},
                                                                           _exceptionStrategy);
            Log.Info($"GetCompetition returned: {result?.Id}.");
            return result;
        }

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <code>culture</code> or a null reference if the sport event with
        /// specified <code>id</code> does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the sport event to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        public ICompetition GetCompetition(URN id, CultureInfo culture = null)
        {
            Log.Info($"Invoked GetCompetition: Id={id}, Culture={culture}");
            return GetCompetition(id, null, culture);
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all fixtures that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(CultureInfo culture = null)
        {
            return GetFixtureChangesAsync(null, null, culture);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitionStatus"/> for specific sport event
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the event for which <see cref="ICompetitionStatus"/> to be retrieved</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<ICompetitionStatus> GetSportEventStatusAsync(URN id)
        {
            Log.Info($"Invoked GetSportEventStatusAsync: Id={id}");
            var sportEventStatusCI = await _sportEventStatusCache.GetSportEventStatusAsync(id).ConfigureAwait(false);
            if (sportEventStatusCI == null)
            {
                return null;
            }

            return new CompetitionStatus(sportEventStatusCI, _matchStatusCache);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitor"/>
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id for which <see cref="ICompetitor"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetitor"/> representing the specified competitor or a null reference</returns>
        public async Task<ICompetitor> GetCompetitorAsync(URN id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetCompetitorAsync: [Id={id}, Cultures={s}].");

            var cacheItem = await _profileCache.GetCompetitorProfileAsync(id, cs).ConfigureAwait(false);
            return cacheItem == null
                       ? null
                       : _sportEntityFactory.BuildCompetitor(cacheItem, cs, (ICompetitionCI) null, _exceptionStrategy);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IPlayerProfile"/>
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id for which <see cref="IPlayerProfile"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="IPlayerProfile"/> representing the specified player or a null reference</returns>
        public async Task<IPlayerProfile> GetPlayerProfileAsync(URN id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + (";" + cultureInfo.TwoLetterISOLanguageName));
            s = s.Substring(1);

            Log.Info($"Invoked GetPlayerProfileAsync: [Id={id}, Cultures={s}].");

            var cacheItem = await _profileCache.GetPlayerProfileAsync(id, cs).ConfigureAwait(false);
            return cacheItem == null
                       ? null
                       : new PlayerProfile(cacheItem, cs);
        }

        /// <summary>
        /// Delete the sport event from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="ISportEvent"/> to be deleted</param>
        /// <param name="includeEventStatusDeletion">Delete also <see cref="ISportEventStatus"/> from the cache</param>
        public void DeleteSportEventFromCache(URN id, bool includeEventStatusDeletion = false)
        {
            Log.Info($"Invoked DeleteSportEventFromCache: Id={id}");
            _cacheManager.RemoveCacheItem(id, CacheItemType.SportEvent, "SportDataProvider");

            if (includeEventStatusDeletion)
            {
                _cacheManager.RemoveCacheItem(id, CacheItemType.SportEventStatus, "SportDataProvider");
            }
        }

        /// <summary>
        /// Delete the tournament from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="ILongTermEvent"/> to be deleted</param>
        public void DeleteTournamentFromCache(URN id)
        {
            Log.Info($"Invoked DeleteTournamentFromCache: Id={id}");
            _cacheManager.RemoveCacheItem(id, CacheItemType.SportEvent, "SportDataProvider");
        }

        /// <summary>
        /// Delete the competitor from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="ICompetitor"/> to be deleted</param>
        public void DeleteCompetitorFromCache(URN id)
        {
            Log.Info($"Invoked DeleteCompetitorFromCache: Id={id}");
            _cacheManager.RemoveCacheItem(id, CacheItemType.Competitor, "SportDataProvider");
        }

        /// <summary>
        /// Delete the player profile from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="IPlayerProfile"/> to be deleted</param>
        public void DeletePlayerProfileFromCache(URN id)
        {
            Log.Info($"Invoked DeletePlayerProfileFromCache: Id={id}");
            _cacheManager.RemoveCacheItem(id, CacheItemType.Player, "SportDataProvider");
        }

        /// <summary>
        /// Asynchronously gets a list of <see cref="IEnumerable{ICompetition}"/>
        /// </summary>
        /// <remarks>Lists almost all events we are offering prematch odds for. This endpoint can be used during early startup to obtain almost all fixtures. This endpoint is one of the few that uses pagination.</remarks>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ICompetition>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture = null)
        {
            if (startIndex < 0)
            {
                throw new ArgumentException("Wrong value", nameof(startIndex));
            }
            if (limit < 1 || limit > 1000)
            {
                throw new ArgumentException("Wrong value", nameof(limit));
            }

            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = string.Join(";", cs);

            Log.Info($"Invoked GetListOfSportEventsAsync: [StartIndex={startIndex}, Limit={limit}, Cultures={s}].");

            var ids = await _dataRouterManager.GetListOfSportEventsAsync(startIndex, limit, culture ?? _defaultCultures.First()).ConfigureAwait(false);

            return ids?.Select(item => _sportEntityFactory.BuildSportEvent<ICompetition>(item.Item1,
                                                                                         item.Item2,
                                                                                         cs,
                                                                                         _exceptionStrategy)).ToList();
        }

        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{ISportEvent}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="ISportEvent"/> that are cached. (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ISportEvent>> GetActiveTournamentsAsync(CultureInfo culture = null)
        {
            Log.Info($"Invoked GetActiveTournamentsAsync: Culture={culture}.");
            var cul = culture ?? _defaultCultures.First();
            var unused = await _sportDataCache.GetSportsAsync(_defaultCultures).ConfigureAwait(false); // to be sure all tournaments for all sports are fetched
            var tours = await _sportEventCache.GetActiveTournamentsAsync(cul).ConfigureAwait(false);
            return tours?.Select(t => _sportEntityFactory.BuildSportEvent<ISportEvent>(t.Id, t.GetSportIdAsync().Result, new[] {cul}, _exceptionStrategy));
        }

        /// <summary>
        /// Asynchronously gets a list of available <see cref="IEnumerable{ISportEvent}"/> for a specific sport
        /// </summary>
        /// <remarks>Lists all available tournaments for a sport event we provide coverage for</remarks>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ISportEvent>> GetAvailableTournamentsAsync(URN sportId, CultureInfo culture = null)
        {
            Log.Info($"Invoked GetAvailableTournamentsAsync: SportId={sportId}, Culture={culture}.");
            var cul = culture ?? _defaultCultures.First();

            var tours = await _dataRouterManager.GetSportAvailableTournamentsAsync(sportId, cul).ConfigureAwait(false);
            return tours?.Select(t => _sportEntityFactory.BuildSportEvent<ISportEvent>(t.Item1, t.Item2, new[] { cul }, _exceptionStrategy));
        }

        /// <summary>
        /// Deletes the sport events from cache which are scheduled before specific DateTime
        /// </summary>
        /// <param name="before">The scheduled DateTime used to delete sport events from cache</param>
        /// <returns>Number of deleted items</returns>
        public int DeleteSportEventsFromCache(DateTime before)
        {
            return _sportEventCache.DeleteSportEventsFromCache(before);
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <param name="cacheType">Specifies what type of cache items will be exported</param>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableCI>> CacheExportAsync(CacheType cacheType)
        {
            var tasks = new List<Task<IEnumerable<ExportableCI>>>();
            if (cacheType.HasFlag(CacheType.SportData))
                tasks.Add((_sportDataCache as IExportableSdkCache).ExportAsync());
            if (cacheType.HasFlag(CacheType.SportEvent))
                tasks.Add((_sportEventCache as IExportableSdkCache).ExportAsync());
            if (cacheType.HasFlag(CacheType.Profile))
                tasks.Add((_profileCache as IExportableSdkCache).ExportAsync());
            tasks.ForEach(t => t.ConfigureAwait(false));
            return (await Task.WhenAll(tasks)).SelectMany(e => e);
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableCI"/> containing the items to be imported</param>
        public Task CacheImportAsync(IEnumerable<ExportableCI> items)
        {
            var cacheItems = items.ToList();
            var tasks = new List<Task>
            {
                (_sportDataCache as IExportableSdkCache).ImportAsync(cacheItems),
                (_sportEventCache as IExportableSdkCache).ImportAsync(cacheItems),
                (_profileCache as IExportableSdkCache).ImportAsync(cacheItems)
            };
            tasks.ForEach(t => t.ConfigureAwait(false));
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IResultChange>> GetResultChangesAsync(CultureInfo culture = null)
        {
            return GetResultChangesAsync(null, null, culture);
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all fixtures that have changed in the last 24 hours</returns>
        public async Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, URN sportId, CultureInfo culture = null)
        {
            culture = culture ?? _defaultCultures.First();

            Log.Info($"Invoked GetFixtureChangesAsync: After={after}, SportId={sportId}, Culture={culture.TwoLetterISOLanguageName}.");

            var result = (await _dataRouterManager.GetFixtureChangesAsync(after, sportId, culture).ConfigureAwait(false))?.ToList();

            Log.Info($"GetFixtureChangesAsync returned {result?.Count} results.");
            return result;
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        public async Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, URN sportId, CultureInfo culture = null)
        {
            culture = culture ?? _defaultCultures.First();

            Log.Info($"Invoked GetResultChangesAsync: After={after}, SportId={sportId}, Culture={culture.TwoLetterISOLanguageName}.");

            var result = (await _dataRouterManager.GetResultChangesAsync(after, sportId, culture).ConfigureAwait(false))?.ToList();

            Log.Info($"GetResultChangesAsync returned {result?.Count} results.");
            return result;
        }
    }
}