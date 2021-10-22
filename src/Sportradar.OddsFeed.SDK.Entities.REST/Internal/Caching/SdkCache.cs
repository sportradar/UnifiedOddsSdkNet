﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Common.Logging;
using Metrics;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Class SdkCache
    /// </summary>
    /// <seealso cref="ISdkCache" />
    internal abstract class SdkCache : ISdkCache
    {
        /// <summary>
        /// Gets the registered dto types
        /// </summary>
        /// <value>The registered dto types</value>
        public IEnumerable<DtoType> RegisteredDtoTypes { get; protected set; }

        /// <summary>
        /// The execution log
        /// </summary>
        protected readonly ILog ExecutionLog;

        /// <summary>
        /// The cache log
        /// </summary>
        protected readonly ILog CacheLog;

        /// <summary>
        /// The cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdkCache"/> class
        /// </summary>
        protected SdkCache(ICacheManager cacheManager)
        {
            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();

            _cacheManager = cacheManager;

            CacheName = GetType().Name;
            // ReSharper disable once VirtualMemberCallInConstructor
            RegisterCache();

            ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(GetType());
            CacheLog = SdkLoggerFactory.GetLoggerForCache(GetType());
        }

        /// <summary>
        /// Gets the name of the cache instance
        /// </summary>
        /// <value>The name</value>
        public string CacheName { get; }

        /// <summary>
        /// Registers the cache in <see cref="CacheManager" />
        /// </summary>
        public void RegisterCache()
        {
            SetDtoTypes();
            if (RegisteredDtoTypes == null)
            {
                throw new InvalidOperationException($"{CacheName} cache has no registered dto types.");
            }
            _cacheManager.RegisterCache(CacheName, this);
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public abstract void SetDtoTypes();

        /// <summary>
        /// Adds the item to the cache
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be added</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        public Task<bool> CacheAddDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(item, nameof(item)).NotNull();

            var timer = Metric.Context($"AddDtoTo_{CacheName}").Timer($"{dtoType}", Unit.Calls);
            using (timer.NewContext($"{id} [{culture.TwoLetterISOLanguageName}]"))
            {
                var syncTask = new Task<bool>(() =>
                {
                    var result = CacheAddDtoItem(id, item, culture, dtoType, requester);
                    return result;
                });

                syncTask.Start();

                return syncTask;
            }
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public abstract void CacheDeleteItem(URN id, CacheItemType cacheItemType);

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public abstract bool CacheHasItem(URN id, CacheItemType cacheItemType);

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A string representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void CacheDeleteItem(string id, CacheItemType cacheItemType)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();

            try
            {
                var urn = URN.Parse(id);
                CacheDeleteItem(urn, cacheItemType);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A string representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool CacheHasItem(string id, CacheItemType cacheItemType)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();

            try
            {
                var urn = URN.Parse(id);
                return CacheHasItem(urn, cacheItemType);
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        /// <summary>
        /// Adds the dto item to cache
        /// </summary>
        /// <param name="id">The identifier of the object</param>
        /// <param name="item">The item to be added</param>
        /// <param name="culture">The culture of the item</param>
        /// <param name="dtoType">Type of the dto</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if added, <c>false</c> otherwise</returns>
        protected abstract bool CacheAddDtoItem(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester);

        /// <summary>
        /// Logs the conflict during saving the DTO instance
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="receivedType">Type of the received.</param>
        /// <param name="logger">The logger.</param>
        protected virtual void LogSavingDtoConflict(URN id, Type expectedType, Type receivedType, ILog logger = null)
        {
            var txt = $"Invalid data for item id={id}. Expecting: {expectedType?.Name}, received: {receivedType?.Name}.";
            if (logger == null)
            {
                ExecutionLog.Warn(txt);
            }
            else
            {
                logger.Warn(txt);
            }
        }

        /// <summary>
        /// Writes the log message
        /// </summary>
        /// <param name="text">The text to me logged</param>
        /// <param name="useDebug">if set to <c>true</c> [use debug].</param>
        protected virtual void WriteLog(string text, bool useDebug = false)
        {
            if (useDebug)
            {
                ExecutionLog.Debug(text);
            }
            else
            {
                ExecutionLog.Info(text);
            }
        }
    }
}
