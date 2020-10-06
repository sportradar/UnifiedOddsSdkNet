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
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    internal class Stage : Competition, IStageV2
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Stage));

        private readonly ISportDataCache _sportDataCache;

        private readonly ISportEntityFactory _sportEntityFactory;
        /// <summary>
        /// The match statuses cache
        /// </summary>
        private readonly ILocalizedNamedValueCache _matchStatusesCache;

        public Stage(URN id,
                    URN sportId,
                    ISportEntityFactory sportEntityFactory,
                    ISportEventCache sportEventCache,
                    ISportDataCache sportDataCache,
                    ISportEventStatusCache sportEventStatusCache,
                    ILocalizedNamedValueCache matchStatusesCache,
                    IEnumerable<CultureInfo> cultures,
                    ExceptionHandlingStrategy exceptionStrategy)
            : base(ExecutionLogPrivate, id, sportId, sportEntityFactory, sportEventStatusCache, sportEventCache, cultures, exceptionStrategy, matchStatusesCache)
        {
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(matchStatusesCache, nameof(matchStatusesCache)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _sportDataCache = sportDataCache;
            _matchStatusesCache = matchStatusesCache;
        }

        public async Task<ISportSummary> GetSportAsync()
        {
            var stageCI = (StageCI) SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            var sportId = await stageCI.GetSportIdAsync().ConfigureAwait(false);
            if (sportId == null)
            {
                ExecutionLog.Debug($"Missing data. No sportId for stage cache item with id={Id}.");
                return null;
            }
            var sportCI = await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false);
            return sportCI == null ? null : new SportSummary(sportCI.Id, sportCI.Names);
        }

        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var stageCI = (StageCI) SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            var categoryId = await stageCI.GetCategoryIdAsync().ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLog.Debug($"Missing data. No categoryId for stage cache item with id={Id}.");
                return null;
            }
            var categoryCI = await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false);
            return categoryCI == null
                ? null
                : new CategorySummary(categoryCI.Id, categoryCI.Names, categoryCI.CountryCode);
        }

        public async Task<IStage> GetParentStageAsync()
        {
            var stageCI = (StageCI) SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }

            var parentStageId = await stageCI.GetParentStageAsync(Cultures).ConfigureAwait(false);
            if (parentStageId != null)
            {
                return new Stage(parentStageId, GetSportAsync().Result.Id, _sportEntityFactory, SportEventCache, _sportDataCache, SportEventStatusCache, _matchStatusesCache, Cultures, ExceptionStrategy);
            }

            return null;
        }

        public async Task<IEnumerable<IStage>> GetStagesAsync()
        {
            var stageCI = (StageCI) SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            var cacheItems = ExceptionStrategy == ExceptionHandlingStrategy.CATCH
                ? await stageCI.GetStagesAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<StageCI>>>(stageCI.GetStagesAsync)
                    .SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("ChildStages")).ConfigureAwait(false);

            return cacheItems?.Select(c => new Stage(c.Id, GetSportAsync().Result.Id, _sportEntityFactory, SportEventCache, _sportDataCache, SportEventStatusCache, _matchStatusesCache, Cultures, ExceptionStrategy));
        }

        /// <summary>
        /// Asynchronously get the type of the stage
        /// </summary>
        /// <returns>The type of the stage</returns>
        async Task<StageType> IStage.GetStageTypeAsync()
        {
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return StageType.Child;
            }
            return await stageCI.GetStageTypeAsync().ConfigureAwait(false) ?? StageType.Child;
        }

        public async Task<StageType?> GetStageTypeAsync()
        {
            var stageCI = (StageCI) SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            return await stageCI.GetStageTypeAsync().ConfigureAwait(false);
        }

        public async Task<SportEventType?> GetEventTypeAsync()
        {
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            return await stageCI.GetSportEventTypeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a list of additional ids of the parent stages of the current instance or a null reference if the represented stage does not have the parent stages
        /// </summary>
        /// <returns>A <see cref="Task{StageCI}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IStage>> GetAdditionalParentStagesAsync()
        {
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            var cacheItems = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await stageCI.GetAdditionalParentStagesAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<URN>>>(stageCI.GetAdditionalParentStagesAsync)
                    .SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("AdditionalParentStages")).ConfigureAwait(false);

            return cacheItems?.Select(c => new Stage(c, GetSportAsync().Result.Id, _sportEntityFactory, SportEventCache, _sportDataCache, SportEventStatusCache, _matchStatusesCache, Cultures, ExceptionStrategy));
        }
    }
}
