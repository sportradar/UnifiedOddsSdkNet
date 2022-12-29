﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Dawn;
using Metrics;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using CacheItemPriority = System.Runtime.Caching.CacheItemPriority;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles
{
    /// <summary>
    /// A <see cref="IProfileCache"/> implementation using <see cref="ObjectCache"/> to cache fetched information
    /// </summary>
    internal class ProfileCache : SdkCache, IProfileCache
    {
        private static readonly ILog LogCache = SdkLoggerFactory.GetLoggerForCache(typeof(ProfileCache));

        /// <summary>
        /// A <see cref="ObjectCache"/> used to store fetched information
        /// </summary>
        private readonly ObjectCache _cache;

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The bag of currently fetching player profiles
        /// </summary>
        private readonly ConcurrentDictionary<URN, DateTime> _fetchedPlayerProfiles = new ConcurrentDictionary<URN, DateTime>();

        /// <summary>
        /// The bag of currently fetching competitor profiles
        /// </summary>
        private readonly ConcurrentDictionary<URN, DateTime> _fetchedCompetitorProfiles = new ConcurrentDictionary<URN, DateTime>();

        /// <summary>
        /// The bag of currently merging ids
        /// </summary>
        private readonly ConcurrentDictionary<URN, DateTime> _mergeUrns = new ConcurrentDictionary<URN, DateTime>();

        /// <summary>
        /// The semaphore used for export/import operation
        /// </summary>
        private readonly SemaphoreSlim _exportSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// The cache item policy
        /// </summary>
        private readonly CacheItemPolicy _simpleTeamCacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.MaxValue, Priority = CacheItemPriority.NotRemovable, RemovedCallback = OnCacheItemRemoval };

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileCache"/> class
        /// </summary>
        /// <param name="cache">A <see cref="ObjectCache"/> used to store fetched information</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch data</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        public ProfileCache(ObjectCache cache,
                            IDataRouterManager dataRouterManager,
                            ICacheManager cacheManager)
                : base(cacheManager)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _cache = cache;
            _dataRouterManager = dataRouterManager;
            _isDisposed = false;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing)
            {
                _fetchedPlayerProfiles.Clear();
                _fetchedCompetitorProfiles.Clear();
                _mergeUrns.Clear();
                _exportSemaphore.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="PlayerProfileCI"/> representing the profile for the specified player
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player for which to get the profile</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        public async Task<PlayerProfileCI> GetPlayerProfileAsync(URN playerId, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(playerId, nameof(playerId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull();
            if (!cultures.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(cultures));
            }

            Metric.Context("CACHE").Meter("ProfileCache->GetPlayerProfileAsync", Unit.Calls);

            await WaitTillIdIsAvailableAsync(_fetchedPlayerProfiles, playerId).ConfigureAwait(false);

            PlayerProfileCI cachedItem;
            try
            {
                cachedItem = (PlayerProfileCI)_cache.Get(playerId.ToString());
                var wantedCultures = cultures.ToList();
                var missingLanguages = LanguageHelper.GetMissingCultures(wantedCultures, cachedItem?.Names.Keys.ToList()).ToList();
                if (!missingLanguages.Any())
                {
                    return cachedItem;
                }

                // try to fetch for competitor, to avoid requests by each player
                if (cachedItem?.CompetitorId != null)
                {
                    await GetCompetitorProfileInsteadOfPlayerProfile(playerId, cachedItem.CompetitorId, missingLanguages);
                }

                cachedItem = (PlayerProfileCI)_cache.Get(playerId.ToString());
                missingLanguages = LanguageHelper.GetMissingCultures(wantedCultures, cachedItem?.Names.Keys.ToList()).ToList();
                if (missingLanguages.Any())
                {
                    var cultureTaskDictionary = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetPlayerProfileAsync(playerId, c, null));
                    await Task.WhenAll(cultureTaskDictionary.Values).ConfigureAwait(false);
                }
                cachedItem = (PlayerProfileCI)_cache.Get(playerId.ToString());
            }
            catch (Exception ex)
            {
                if (ex is DeserializationException || ex is MappingException)
                {
                    throw new CacheItemNotFoundException($"An error occurred while fetching player profile for player {playerId} in cache", playerId.ToString(), ex);
                }
                throw;
            }
            finally
            {
                await ReleaseIdAsync(_fetchedPlayerProfiles, playerId).ConfigureAwait(false);
            }
            return cachedItem;
        }

        private async Task GetCompetitorProfileInsteadOfPlayerProfile(URN playerId, URN competitorId, IReadOnlyCollection<CultureInfo> wantedCultures)
        {
            if (competitorId == null || wantedCultures.IsNullOrEmpty())
            {
                return;
            }
            var competitorCI = (CompetitorCI)_cache.Get(competitorId.ToString());
            if (competitorCI == null)
            {
                ExecutionLog.Debug($"Fetching competitor profile for competitor {competitorId} instead of player {playerId} for languages=[{LanguageHelper.GetCultureList(wantedCultures)}].");
                _ = await GetCompetitorProfileAsync(competitorId, wantedCultures).ConfigureAwait(false);
            }
            else
            {
                var missingCultures = competitorCI.GetMissingProfileCultures(wantedCultures);
                if (!missingCultures.IsNullOrEmpty())
                {
                    ExecutionLog.Debug($"Fetching competitor profile for competitor {competitorId} instead of player {playerId} for languages=[{LanguageHelper.GetCultureList(missingCultures)}].");
                    _ = await GetCompetitorProfileAsync(competitorId, missingCultures).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Asynchronously gets <see cref="CompetitorCI"/> representing the profile for the specified competitor
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        public async Task<CompetitorCI> GetCompetitorProfileAsync(URN competitorId, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(competitorId, nameof(competitorId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull();//.NotEmpty();
            if (!cultures.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(cultures));
            }

            Metric.Context("CACHE").Meter("ProfileCache->GetCompetitorProfileAsync", Unit.Calls);

            await WaitTillIdIsAvailableAsync(_fetchedCompetitorProfiles, competitorId).ConfigureAwait(false);

            CompetitorCI cachedItem;
            try
            {
                var missingLanguages = cultures.ToList();
                cachedItem = (CompetitorCI)_cache.Get(competitorId.ToString());
                if (cachedItem != null)
                {
                    missingLanguages = cachedItem.GetMissingProfileCultures(new ReadOnlyCollection<CultureInfo>(missingLanguages)).ToList();
                }
                if (missingLanguages.Any())
                {
                    var cultureTasks = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetCompetitorAsync(competitorId, c, null));
                    await Task.WhenAll(cultureTasks.Values).ConfigureAwait(false);
                }
                cachedItem = (CompetitorCI)_cache.Get(competitorId.ToString());
            }
            catch (Exception ex)
            {
                if (ex is DeserializationException || ex is MappingException)
                {
                    throw new CacheItemNotFoundException("An error occurred while fetching competitor profile not found in cache", competitorId.ToString(), ex);
                }
                throw;
            }
            finally
            {
                await ReleaseIdAsync(_fetchedCompetitorProfiles, competitorId).ConfigureAwait(false);
            }
            return cachedItem;
        }

        private async Task WaitTillIdIsAvailableAsync(ConcurrentDictionary<URN, DateTime> bag, URN id)
        {
            var stopwatch = Stopwatch.StartNew();
            var expireDate = DateTime.Now.AddSeconds(30);
            while (bag.ContainsKey(id) && DateTime.Now < expireDate)
            {
                await Task.Delay(25).ConfigureAwait(false);
            }
            if (!bag.ContainsKey(id))
            {
                bag.TryAdd(id, DateTime.Now);
            }
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                LogCache.Debug($"WaitTillIdIsAvailable for {id} took {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private async Task ReleaseIdAsync(ConcurrentDictionary<URN, DateTime> bag, URN id)
        {
            var expireDate = DateTime.Now.AddSeconds(5);
            while (bag.ContainsKey(id) && DateTime.Now < expireDate)
            {
                if (!bag.TryRemove(id, out _))
                {
                    await Task.Delay(25).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            HealthChecks.RegisterHealthCheck(CacheName, StartHealthCheck);
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        public HealthCheckResult StartHealthCheck()
        {
            var keys = _cache.Select(w => w.Key).ToList();
            var details = $" [Players: {keys.Count(c => c.Contains("player"))}, Competitors: {keys.Count(c => c.Contains("competitor"))}, Teams: {keys.Count(c => c.Equals("team"))}, SimpleTeams: {keys.Count(URN.IsSimpleTeam)}]";
            return _cache.Any() ? HealthCheckResult.Healthy($"Cache has {_cache.Count()} items{details}.") : HealthCheckResult.Unhealthy("Cache is empty.");
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Fixture,
                                     DtoType.MatchTimeline,
                                     DtoType.Competitor,
                                     DtoType.CompetitorProfile,
                                     DtoType.SimpleTeamProfile,
                                     DtoType.PlayerProfile,
                                     DtoType.SportEventSummary,
                                     DtoType.TournamentInfo,
                                     DtoType.RaceSummary,
                                     DtoType.MatchSummary,
                                     DtoType.TournamentInfoList,
                                     DtoType.TournamentSeasons,
                                     DtoType.SportEventSummaryList
                                 };
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(URN id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All
                || cacheItemType == CacheItemType.Competitor
                || cacheItemType == CacheItemType.Player)
            {
                _cache.Remove(id.ToString());
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(URN id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All
                || cacheItemType == CacheItemType.Competitor
                || cacheItemType == CacheItemType.Player)
            {
                return _cache.Contains(id.ToString());
            }
            return false;
        }

        private static void OnCacheItemRemoval(CacheEntryRemovedArguments arguments)
        {
            if (arguments.RemovedReason != CacheEntryRemovedReason.Removed && arguments.RemovedReason != CacheEntryRemovedReason.CacheSpecificEviction)
            {
                LogCache.Debug($"{arguments.RemovedReason} from cache: {arguments.CacheItem.Key}");
            }
        }

        protected override async Task<bool> CacheAddDtoItemAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.MatchSummary:
                    var competitorsSaved1 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved1)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDTO), item.GetType());
                    }
                    break;
                case DtoType.RaceSummary:
                    var competitorsSaved2 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved2)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    var competitorsSaved3 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved3)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDTO), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummary:
                    var competitorsSaved4 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved4)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDTO), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    break;
                case DtoType.Category:
                    break;
                case DtoType.Tournament:
                    break;
                case DtoType.PlayerProfile:
                    var playerProfile = item as PlayerProfileDTO;
                    if (playerProfile != null)
                    {
                        await AddPlayerProfileAsync(playerProfile, null, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(PlayerProfileDTO), item.GetType());
                    }
                    break;
                case DtoType.Competitor:
                    var competitor = item as CompetitorDTO;
                    if (competitor != null)
                    {
                        await AddCompetitorAsync(id, competitor, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CompetitorDTO), item.GetType());
                    }
                    break;
                case DtoType.CompetitorProfile:
                    var competitorProfile = item as CompetitorProfileDTO;
                    if (competitorProfile != null)
                    {
                        await AddCompetitorProfileAsync(id, competitorProfile, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CompetitorProfileDTO), item.GetType());
                    }
                    break;
                case DtoType.SimpleTeamProfile:
                    var simpleTeamProfile = item as SimpleTeamProfileDTO;
                    if (simpleTeamProfile != null)
                    {
                        await AddCompetitorProfileAsync(id, simpleTeamProfile, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SimpleTeamProfileDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescription:
                    break;
                case DtoType.SportEventStatus:
                    break;
                case DtoType.MatchTimeline:
                    var matchTimeline = item as MatchTimelineDTO;
                    if (matchTimeline != null)
                    {
                        saved = await SaveCompetitorsFromSportEventAsync(matchTimeline.SportEvent, culture).ConfigureAwait(false);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    var tournamentSeason = item as TournamentSeasonsDTO;
                    if (tournamentSeason?.Tournament != null)
                    {
                        await SaveCompetitorsFromSportEventAsync(tournamentSeason.Tournament, culture).ConfigureAwait(false);
                        saved = true;
                    }
                    break;
                case DtoType.Fixture:
                    var competitorsSaved5 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved5)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDTO), item.GetType());
                    }
                    break;
                case DtoType.SportList:
                    break;
                case DtoType.SportEventSummaryList:
                    var sportEventSummaryList = item as EntityList<SportEventSummaryDTO>;
                    if (sportEventSummaryList != null)
                    {
                        var tasks = sportEventSummaryList.Items.Select(s => SaveCompetitorsFromSportEventAsync(s, culture));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    break;
                case DtoType.LotteryDraw:
                    break;
                case DtoType.LotteryList:
                    break;
                case DtoType.BookingStatus:
                    break;
                case DtoType.SportCategories:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    var ts = item as EntityList<TournamentInfoDTO>;
                    if (ts != null)
                    {
                        var tasks = ts.Items.Select(s => SaveCompetitorsFromSportEventAsync(s, culture));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<TournamentInfoDTO>), item.GetType());
                    }
                    break;
                default:
                    ExecutionLog.Warn($"Trying to add unchecked dto type: {dtoType} for id: {id}.");
                    break;
            }

            return saved;
        }

        private async Task<bool> SaveCompetitorsFromSportEventAsync(object item, CultureInfo culture)
        {
            if (item is FixtureDTO fixture)
            {
                if (fixture.Competitors != null && fixture.Competitors.Any())
                {
                    var tasks = fixture.Competitors.Select(s => AddTeamCompetitorAsync(s.Id, s, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                return true;
            }

            if (item is MatchDTO match)
            {
                if (match.Competitors != null && match.Competitors.Any())
                {
                    var tasks = match.Competitors.Select(s => AddTeamCompetitorAsync(s.Id, s, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                return true;
            }

            if (item is StageDTO stage)
            {
                if (stage.Competitors != null && stage.Competitors.Any())
                {
                    var tasks = stage.Competitors.Select(s => AddTeamCompetitorAsync(s.Id, s, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                return true;
            }

            if (item is TournamentInfoDTO tour)
            {
                if (tour.Competitors != null && tour.Competitors.Any())
                {
                    var tasks = tour.Competitors.Select(s => AddCompetitorAsync(s.Id, s, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                if (tour.Groups != null && tour.Groups.Any())
                {
                    foreach (var tourGroup in tour.Groups)
                    {
                        if (tourGroup.Competitors != null && tourGroup.Competitors.Any())
                        {
                            var tasks = tourGroup.Competitors.Select(s => AddCompetitorAsync(s.Id, s, culture, true));
                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private async Task AddTeamCompetitorAsync(URN id, TeamCompetitorDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI)_cache.Get(id.ToString());
                    var teamCI = ci == null ? (TeamCompetitorCI)_cache.Get(id.ToString()) : ci as TeamCompetitorCI;
                    if (teamCI != null)
                    {
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, id).ConfigureAwait(false);
                        }
                        teamCI.Merge(item, culture);
                    }
                    else
                    {
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, id).ConfigureAwait(false);
                        }
                        teamCI = new TeamCompetitorCI(ci);
                        teamCI.Merge(item, culture);
                        _cache.Set(id.ToString(), teamCI, GetCorrectCacheItemPolicy(id));
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error adding team competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.", ex);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var teamCompetitor = new TeamCompetitorCI(item, culture, _dataRouterManager);
                _cache.Add(id.ToString(), teamCompetitor, GetCorrectCacheItemPolicy(id));
            }

            if (item?.Players != null && item.Players.Any())
            {
                var tasks = item.Players.Select(s => AddPlayerCompetitorAsync(s, item.Id, culture, false));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task AddTeamCompetitorAsync(ExportableTeamCompetitorCI item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = URN.Parse(item.Id);
                try
                {
                    var ci = (CompetitorCI)_cache.Get(item.Id);
                    var teamCI = ci == null ? (TeamCompetitorCI)_cache.Get(item.Id) : ci as TeamCompetitorCI;
                    if (teamCI != null)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                        teamCI.Import(item);
                    }
                    else
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                        teamCI = new TeamCompetitorCI(ci);
                        teamCI.Import(item);
                        _cache.Set(item.Id, teamCI, GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error importing team competitor for id={item.Id}.", ex);
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id, new TeamCompetitorCI(item, _dataRouterManager), GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
            }
        }

        private async Task AddCompetitorAsync(URN id, CompetitorDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI)_cache.Get(id.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                    ci.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error adding competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.", ex);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));
            }

            if (item?.Players != null && item.Players.Any())
            {
                var tasks = item.Players.Select(s => AddPlayerCompetitorAsync(s, item.Id, culture, false));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task AddCompetitorAsync(ExportableCompetitorCI item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = URN.Parse(item.Id);
                try
                {
                    var ci = (CompetitorCI)_cache.Get(item.Id);
                    await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    ci.Import(item);
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error importing competitor for id={item.Id}.", ex);
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.Id))
                {
                    _cache.Add(item.Id, new CompetitorCI(item, _dataRouterManager), GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
                }
            }
        }

        private async Task AddCompetitorProfileAsync(URN id, CompetitorProfileDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI)_cache.Get(id.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                    ci?.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error adding competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.", ex);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));
            }

            if (item?.Players != null && item.Players.Any())
            {
                var tasks = item.Players.Select(s => AddPlayerProfileAsync(s, id, culture, true));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task AddCompetitorProfileAsync(URN id, SimpleTeamProfileDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI)_cache.Get(id.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                    ci.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error adding competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.", ex);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));
            }
        }

        private async Task AddPlayerProfileAsync(PlayerProfileDTO item, URN competitorId, CultureInfo culture, bool useSemaphore)
        {
            if (item == null)
            {
                return;
            }
            if (_cache.Contains(item.Id.ToString()))
            {
                try
                {
                    var ci = (PlayerProfileCI)_cache.Get(item.Id.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                    }
                    ci.Merge(item, competitorId, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error adding player profile for id={item.Id}, dto type={item.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.", ex);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id.ToString(), new PlayerProfileCI(item, competitorId, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
            }
        }

        private async Task AddPlayerProfileAsync(ExportablePlayerProfileCI item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = URN.Parse(item.Id);
                try
                {
                    var ci = (PlayerProfileCI)_cache.Get(item.Id);
                    await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    ci.Import(item);
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error importing player profile for id={item.Id}.", ex);
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id, new PlayerProfileCI(item, _dataRouterManager), GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
            }
        }

        private async Task AddPlayerCompetitorAsync(PlayerCompetitorDTO item, URN competitorId, CultureInfo culture, bool useSemaphore)
        {
            if (item == null)
            {
                return;
            }
            if (_cache.Contains(item.Id.ToString()))
            {
                try
                {
                    if (item.Id.Type.Equals(SdkInfo.PlayerProfileIdentifier, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var ci = (PlayerProfileCI)_cache.Get(item.Id.ToString());
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                        }

                        ci.Merge(item, competitorId, culture);
                    }
                    else
                    {
                        var ci = (CompetitorCI)_cache.Get(item.Id.ToString());
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                        }

                        ci.Merge(item, culture);
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"Error adding player profile for id={item.Id}, dto type={item.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.", ex);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                if (item.Id.Type.Equals(SdkInfo.PlayerProfileIdentifier, StringComparison.InvariantCultureIgnoreCase))
                {
                    _cache.Add(item.Id.ToString(), new PlayerProfileCI(item, competitorId, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
                }
                else
                {
                    _cache.Add(item.Id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
                }
            }
        }

        private CacheItemPolicy GetCorrectCacheItemPolicy(URN id)
        {
            return id.IsSimpleTeam()
                ? _simpleTeamCacheItemPolicy
                : new CacheItemPolicy { SlidingExpiration = SdkInfo.AddVariableNumber(OperationManager.ProfileCacheTimeout, 20), RemovedCallback = OnCacheItemRemoval };
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableCI>> ExportAsync()
        {
            IEnumerable<IExportableCI> exportables;
            try
            {
                await _exportSemaphore.WaitAsync();
                exportables = _cache.Select(i => (IExportableCI)i.Value);
            }
            finally
            {
                _exportSemaphore.ReleaseSafe();
            }

            var tasks = exportables.Select(e =>
            {
                var task = e.ExportAsync();
                task.ConfigureAwait(false);
                return task;
            });

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableCI"/> to be inserted into the cache</param>
        public async Task ImportAsync(IEnumerable<ExportableCI> items)
        {
            foreach (var exportable in items)
            {
                if (exportable is ExportableTeamCompetitorCI exportableTeamCompetitor)
                {
                    await AddTeamCompetitorAsync(exportableTeamCompetitor).ConfigureAwait(false);
                    continue;
                }
                if (exportable is ExportableCompetitorCI exportableCompetitor)
                {
                    await AddCompetitorAsync(exportableCompetitor).ConfigureAwait(false);
                    continue;
                }
                if (exportable is ExportablePlayerProfileCI exportablePlayerProfile)
                {
                    await AddPlayerProfileAsync(exportablePlayerProfile).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        public IReadOnlyDictionary<string, int> CacheStatus()
        {
            List<KeyValuePair<string, object>> items;
            try
            {
                _exportSemaphore.Wait();
                items = _cache.ToList();
            }
            finally
            {
                _exportSemaphore.ReleaseSafe();
            }

            return new Dictionary<string, int>
            {
                {typeof(TeamCompetitorCI).Name, items.Count(i => i.Value.GetType() == typeof(TeamCompetitorCI))},
                {typeof(CompetitorCI).Name, items.Count(i => i.Value.GetType() == typeof(CompetitorCI))},
                {typeof(PlayerProfileCI).Name, items.Count(i => i.Value.GetType() == typeof(PlayerProfileCI))}
            };
        }
    }
}
