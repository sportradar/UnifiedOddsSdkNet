﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Dawn;
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
    internal class Stage : Competition, IStageV3
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Stage));

        private readonly ISportDataCache _sportDataCache;

        private readonly ISportEntityFactory _sportEntityFactory;

        public Stage(URN id,
                    URN sportId,
                    ISportEntityFactory sportEntityFactory,
                    ISportEventCache sportEventCache,
                    ISportDataCache sportDataCache,
                    ISportEventStatusCache sportEventStatusCache,
                    ILocalizedNamedValueCache matchStatusCache,
                    IReadOnlyCollection<CultureInfo> cultures,
                    ExceptionHandlingStrategy exceptionStrategy)
            : base(ExecutionLogPrivate, id, sportId, sportEntityFactory, sportEventStatusCache, sportEventCache, cultures, exceptionStrategy, matchStatusCache)
        {
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(matchStatusCache, nameof(matchStatusCache)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _sportDataCache = sportDataCache;
        }

        public async Task<ISportSummary> GetSportAsync()
        {
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
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
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            var categoryId = await stageCI.GetCategoryIdAsync(Cultures).ConfigureAwait(false);
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
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }

            var parentStageId = await stageCI.GetParentStageAsync(Cultures).ConfigureAwait(false);
            if (parentStageId != null)
            {
                return new Stage(parentStageId, GetSportAsync().GetAwaiter().GetResult().Id, _sportEntityFactory, SportEventCache, _sportDataCache, SportEventStatusCache, MatchStatusCache, Cultures, ExceptionStrategy);
            }

            return null;
        }

        public async Task<IEnumerable<IStage>> GetStagesAsync()
        {
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            var cacheItems = ExceptionStrategy == ExceptionHandlingStrategy.CATCH
                ? await stageCI.GetStagesAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<URN>>>(stageCI.GetStagesAsync)
                    .SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("ChildStages")).ConfigureAwait(false);

            return cacheItems?.Select(c => new Stage(c, GetSportAsync().GetAwaiter().GetResult().Id, _sportEntityFactory, SportEventCache, _sportDataCache, SportEventStatusCache, MatchStatusCache, Cultures, ExceptionStrategy));
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

            var result = await stageCI.GetStageTypeAsync().ConfigureAwait(false);
            return result ?? StageType.Child;
        }

        public async Task<StageType?> GetStageTypeAsync()
        {
            var stageCI = (StageCI)SportEventCache.GetEventCacheItem(Id);
            if (stageCI == null)
            {
                ExecutionLog.Debug($"Missing data. No stage cache item for id={Id}.");
                return null;
            }
            return await stageCI.GetStageTypeAsync().ConfigureAwait(false);
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

            return cacheItems?.Select(c => new Stage(c, GetSportAsync().GetAwaiter().GetResult().Id, _sportEntityFactory, SportEventCache, _sportDataCache, SportEventStatusCache, MatchStatusCache, Cultures, ExceptionStrategy));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IStageStatus"/> containing information about the progress of the stage 
        /// </summary>
        /// <returns>A <see cref="Task{IStageStatus}"/> containing information about the progress of the stage</returns>
        public new async Task<IStageStatus> GetStatusAsync()
        {
            var item = await base.GetStatusAsync().ConfigureAwait(false);
            return item == null ? null : new StageStatus(((CompetitionStatus)item).SportEventStatusCI, MatchStatusCache);
        }
    }
}
