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
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Class CacheManager
    /// </summary>
    /// <seealso cref="ICacheManager" />
    internal class CacheManager : ICacheManager
    {
        private static readonly ILog ExecLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CacheManager));

        private Dictionary<string, ISdkCache> _caches;

        /// <summary>
        /// Registers the cache in the CacheManager
        /// </summary>
        /// <param name="name">The name of the instance</param>
        /// <param name="cache">The cache to be registered</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3928:Parameter names used into ArgumentException constructors should match an existing one ", Justification = "Invalid argument")]
        public void RegisterCache(string name, ISdkCache cache)
        {
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
            Guard.Argument(cache, nameof(cache)).NotNull();

            if (_caches == null)
            {
                _caches = new Dictionary<string, ISdkCache>();
            }
            if (cache.RegisteredDtoTypes == null || !cache.RegisteredDtoTypes.Any())
            {
                throw new ArgumentException("Missing registered dto types", nameof(cache.RegisteredDtoTypes));
            }
            if (_caches.ContainsKey(name))
            {
                ExecLog.Warn($"Cache with the name={name} already added. Removing it.");
                _caches.Remove(name);
            }
            ExecLog.Debug($"Registering cache with the name={name} to the CacheManager.");
            _caches.Add(name, cache);
        }

        /// <summary>
        /// Adds the item to the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be add</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        public void SaveDto(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            SaveDtoAsync(id, item, culture, dtoType, requester).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds the item to the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be add</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        public async Task SaveDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(item, nameof(item)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (_caches == null || !_caches.Any())
            {
                return;
            }

            var appropriateCaches = _caches.Where(s => s.Value.RegisteredDtoTypes.Contains(dtoType)).ToList();

            if (!appropriateCaches.Any())
            {
                ExecLog.Debug($"No cache with registered type:{dtoType} and lang:[{culture.TwoLetterISOLanguageName}] to save data.");
                return;
            }

            var tasks = appropriateCaches.Select(c => c.Value.CacheAddDtoAsync(id, item, culture, dtoType, requester)).ToArray();
            if (tasks.Any())
            {
                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    ExecLog.Error($"Error saving dto data for id={id}, lang=[{culture.TwoLetterISOLanguageName}], type={dtoType}.", e);
                }
            }
            else
            {
                ExecLog.Debug("Cannot save data. There is no registered cache.");
            }
        }

        /// <summary>
        /// Remove the cache item in the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="cacheItemType">Type of the cache item</param>
        /// <param name="sender">The name of the cache or class that is initiating request</param>
        public void RemoveCacheItem(URN id, CacheItemType cacheItemType, string sender)
        {
            Guard.Argument(id, nameof(id)).NotNull();

            if (_caches == null || !_caches.Any())
            {
                ExecLog.Warn("Cannot remove item from cache. There is no registered cache.");
                return;
            }

            var caches = _caches.Where(c => !c.Value.CacheName.Equals(sender));
            foreach (var cache in caches)
            {
                cache.Value.CacheDeleteItem(id, cacheItemType);
            }
        }
    }
}
