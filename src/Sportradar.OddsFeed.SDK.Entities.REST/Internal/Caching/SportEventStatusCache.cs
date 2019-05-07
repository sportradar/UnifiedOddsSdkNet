/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Internal.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Implementation of <see cref="ISportEventStatusCache"/>
    /// </summary>
    /// <seealso cref="ISportEventStatusCache" />
    internal class SportEventStatusCache : SdkCache, ISportEventStatusCache, IHealthStatusProvider, IDisposable
    {
        /// <summary>
        /// A <see cref="ISingleTypeMapperFactory{restSportEventStatus, SportEventStatusDTO}"/> used to created <see cref="ISingleTypeMapper{SportEventStatusDTO}"/> instances
        /// </summary>
        private readonly ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO> _mapperFactory;

        /// <summary>
        /// A <see cref="ObjectCache"/> used to cache <see cref="ISportEventStatus"/> instances
        /// </summary>
        private readonly ObjectCache _sportEventStatusCache;

        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> used to synchronize multi-threaded fetching
        /// </summary>
        private readonly SemaphoreSlim _fetchSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// A <see cref="object"/> to ensure thread safety when adding items to cache
        /// </summary>
        private readonly object _addLock = new object();

        /// <summary>
        /// The cache item expire time
        /// </summary>
        private readonly TimeSpan _cacheItemExpireTime;

        // Used for collecting eventIds for which GetSummary was called from within this cache, to avoid deadlock
        private readonly BlockingCollection<URN> _doNotUseSemaphoreForId = new BlockingCollection<URN>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ISportEventStatusCache"/> class
        /// </summary>
        /// <param name="sportEventStatusCache"> A <see cref="ObjectCache"/> used to cache <see cref="SportEventStatusCI"/> instances</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{TIn,TOut}"/> used to created <see cref="ISingleTypeMapper{SportEventStatusDTO}"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to cache <see cref="ISportEvent"/></param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="cacheItemExpireTime">The time in which cache item expires</param>
        public SportEventStatusCache(ObjectCache sportEventStatusCache,
                                    ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO> mapperFactory,
                                    ISportEventCache sportEventCache,
                                    ICacheManager cacheManager,
                                    TimeSpan cacheItemExpireTime)
            : base(cacheManager)
        {
            Contract.Requires(sportEventStatusCache != null);
            Contract.Requires(mapperFactory != null);
            Contract.Requires(sportEventCache != null);

            _sportEventStatusCache = sportEventStatusCache;
            _mapperFactory = mapperFactory;
            _sportEventCache = sportEventCache;

            _isDisposed = false;
            _cacheItemExpireTime = cacheItemExpireTime <= TimeSpan.Zero ? TimeSpan.FromMinutes(5) : cacheItemExpireTime;
        }

        /// <summary>
        /// Gets the cached <see cref="SportEventStatusCI" /> instance associated with the sport event specified by the <code>eventId</code>. If the instance associated
        /// with the specified event is not found, it tries to obtain it via API, if still cant, a <see cref="SportEventStatusCI" /> instance indicating a 'not started' event is returned.
        /// </summary>
        /// <param name="eventId">A <see cref="URN" /> representing the id of the sport event whose status to get</param>
        /// <returns>A <see cref="SportEventStatusCI" /> representing the status of the specified sport event</returns>
        public async Task<SportEventStatusCI> GetSportEventStatusAsync(URN eventId)
        {
            if (_isDisposed)
            {
                return null;
            }


            //if not, create NotStarted
            SportEventStatusCI ci = null;
            try
            {
                lock (_addLock)
                {
                    // get from cache
                    var item = _sportEventStatusCache.Get(eventId.ToString());

                    if (item != null)
                    {
                        return (SportEventStatusCI) item;
                    }
                }

                await _fetchSemaphore.WaitAsync().ConfigureAwait(false);

                // fetch from api
                var cachedEvent = _sportEventCache.GetEventCacheItem(eventId) as ICompetitionCI;
                _doNotUseSemaphoreForId.TryAdd(eventId);
                var cachedStatus = cachedEvent == null
                    ? null
                    : await cachedEvent.GetSportEventStatusAsync().ConfigureAwait(false);
                if (cachedStatus != null)
                {
                    ci = cachedStatus;
                }
                Metric.Context("CACHE").Meter("SportEventStatusCache->GetSportEventStatusAsync", Unit.Calls).Mark();
            }
            finally
            {
                if (!_isDisposed)
                {
                    _fetchSemaphore.Release();
                }
                URN takenEventId;
                _doNotUseSemaphoreForId.TryTake(out takenEventId);
            }

            if (ci == null)
            {
                ci = ((SportEventStatusMapperBase)_mapperFactory).CreateNotStarted();
            }

            return ci;
        }

        /// <summary>
        /// Adds the sport event status to the internal cache
        /// </summary>
        /// <param name="eventId">The eventId of the sport event status to be cached</param>
        /// <param name="sportEventStatus">The sport event status to be cached</param>
        /// <param name="source">The source of the SES</param>
        private void AddSportEventStatus(URN eventId, SportEventStatusCI sportEventStatus, string source)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (_addLock)
            {
                if (string.IsNullOrEmpty(source) ||
                    source.Equals("OddsChange", StringComparison.InvariantCultureIgnoreCase) ||
                    source.Equals("SportEventSummary", StringComparison.InvariantCultureIgnoreCase) ||
                    !_sportEventStatusCache.Contains(eventId.ToString()))
                {
                    if (!string.IsNullOrEmpty(source))
                    {
                        source = $" from {source}";
                    }
                    ExecutionLog.Debug($"Received SES for {eventId}{source} with EventStatus:{sportEventStatus.Status}");
                    var cacheItem = _sportEventStatusCache.AddOrGetExisting(
                        eventId.ToString(),
                        sportEventStatus,
                        new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(_cacheItemExpireTime.TotalSeconds)
                        }) as SportEventStatusCI;
                    if (cacheItem != null)
                    {
                        cacheItem.FeedDTO = sportEventStatus.FeedDTO ?? cacheItem.FeedDTO;
                        cacheItem.SapiDTO = sportEventStatus.SapiDTO ?? cacheItem.SapiDTO;
                    }
                }
                else
                {
                    ExecutionLog.Debug($"Received SES for {eventId} from {source} with EventStatus:{sportEventStatus.Status} (ignored)");
                }
            }
        }

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            HealthChecks.RegisterHealthCheck("SportEventStatusCache", new Func<HealthCheckResult>(StartHealthCheck));
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        public HealthCheckResult StartHealthCheck()
        {
            lock (_addLock)
            {
                return _sportEventStatusCache.Any() ? HealthCheckResult.Healthy($"Cache has {_sportEventStatusCache.Count()} items.") : HealthCheckResult.Unhealthy("Cache is empty.");
            }
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Fixture,
                                     DtoType.MatchSummary,
                                     DtoType.MatchTimeline,
                                     DtoType.RaceSummary,
                                     DtoType.SportEventStatus,
                                     DtoType.SportEventSummary,
                                     DtoType.SportEventSummaryList
                                 };
        }

        /// <summary>
        /// Purges item from cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item in the cache to be purged</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(URN id, CacheItemType cacheItemType)
        {
            if (_isDisposed)
            {
                return;
            }

            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.SportEventStatus)
            {
                lock (_addLock)
                {
                    _sportEventStatusCache.Remove(id.ToString());
                }
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
            if (_isDisposed)
            {
                return false;
            }
            var result = false;
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.SportEventStatus)
            {
                lock (_addLock)
                {
                    result = _sportEventStatusCache.Contains(id.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Adds the dto item to cache
        /// </summary>
        /// <param name="id">The identifier of the object</param>
        /// <param name="item">The item</param>
        /// <param name="culture">The culture</param>
        /// <param name="dtoType">Type of the dto</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if added, <c>false</c> otherwise</returns>
        protected override bool CacheAddDtoItem(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.Category:
                    break;
                case DtoType.Competitor:
                    break;
                case DtoType.CompetitorProfile:
                    break;
                case DtoType.SimpleTeamProfile:
                    break;
                case DtoType.Fixture:
                    var fixtureDTO = item as FixtureDTO;
                    if (fixtureDTO != null)
                    {
                        if (fixtureDTO.Status != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCI(fixtureDTO.Status, null), "Fixture");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescription:
                    break;
                case DtoType.MatchSummary:
                    var matchDTO = item as MatchDTO;
                    if (matchDTO != null)
                    {
                        if (matchDTO.Status != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCI(null, matchDTO.Status), "Match");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDTO), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    var matchTimelineDTO = item as MatchTimelineDTO;
                    if (matchTimelineDTO != null)
                    {
                        if (matchTimelineDTO.SportEventStatus != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCI(null, matchTimelineDTO.SportEventStatus), "MatchTimeline");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDTO), item.GetType());
                    }
                    break;
                case DtoType.PlayerProfile:
                    break;
                case DtoType.RaceSummary:
                    var stageDTO = item as StageDTO;
                    if (stageDTO != null)
                    {
                        if (stageDTO.Status != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCI(null, stageDTO.Status), "Stage");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDTO), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    break;
                case DtoType.SportList:
                    break;
                case DtoType.SportEventStatus:
                    var sportEventStatusDTO = item as SportEventStatusDTO;
                    if (sportEventStatusDTO != null)
                    {
                        AddSportEventStatus(id, new SportEventStatusCI(sportEventStatusDTO, null), "OddsChange");
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventStatusDTO), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummary:
                    var competitionDTO = item as CompetitionDTO;
                    if (competitionDTO != null)
                    {
                        if (competitionDTO.Status != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCI(null, competitionDTO.Status), "SportEventSummary");
                        }
                        saved = true;
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    var summaryList = item as EntityList<SportEventSummaryDTO>;
                    if (summaryList != null)
                    {
                        foreach (var s in summaryList.Items)
                        {
                            var compDTO = s as CompetitionDTO;
                            if (compDTO?.Status != null)
                            {
                                AddSportEventStatus(id, new SportEventStatusCI(null, compDTO.Status), "SportEventSummaryList");
                            }
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportEventSummaryDTO>), item.GetType());
                    }
                    break;
                case DtoType.Tournament:
                    break;
                case DtoType.TournamentInfo:
                    break;
                case DtoType.TournamentSeasons:
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
                default:
                    ExecutionLog.Warn($"Trying to add unchecked dto type: {dtoType} for id: {id}.");
                    break;
            }
            //CacheLog.Debug($"Saving {id} COMPLETED. Saved={saved}.");
            return saved;
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
                _fetchSemaphore.Dispose();
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
    }
}