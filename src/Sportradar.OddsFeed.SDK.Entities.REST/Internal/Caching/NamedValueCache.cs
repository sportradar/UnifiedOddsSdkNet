﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Dawn;
using Metrics;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// A default implementation of the <see cref="INamedValueCache"/>
    /// </summary>
    /// <seealso cref="INamedValueCache" />
    internal class NamedValueCache : INamedValueCache
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for logging
        /// </summary>
        private static readonly ILog CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(NamedValueCache));

        /// <summary>
        /// The <see cref="ILog"/> instance used for execution logging.
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(NamedValueCache));

        /// <summary>
        /// A <see cref="IDataProvider{T}"/> used to get the named values
        /// </summary>
        private readonly IDataProvider<EntityList<NamedValueDTO>> _dataProvider;

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing the match status translations
        /// </summary>
        private readonly IDictionary<int, string> _namedValues;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        private readonly string _cacheName;

        /// <summary>
        /// An <see cref="object"/> used to sync  access to shared members
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// A value indicating whether the data was already fetched
        /// </summary>
        private bool _dataFetched;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueCache"/> class.
        /// </summary>
        /// <param name="dataProvider">A <see cref="IDataProvider{T}"/> used to get the named values</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled</param>
        /// <param name="cacheName">A name of the cache or the name of the values contained in this cache</param>
        public NamedValueCache(IDataProvider<EntityList<NamedValueDTO>> dataProvider, ExceptionHandlingStrategy exceptionStrategy, string cacheName)
        {
            Guard.Argument(dataProvider, nameof(dataProvider)).NotNull();

            _dataProvider = dataProvider;
            _namedValues = new Dictionary<int, string>();
            _exceptionStrategy = exceptionStrategy;
            _cacheName = cacheName;

            Task.Run(() =>
                     {
                         Task.Delay(300).GetAwaiter().GetResult();
                         IsValueDefined(1);
                     });
        }

        /// <summary>
        /// Asynchronously gets a match stats descriptions specified by the language specified by <code>culture</code>
        /// </summary>
        /// <returns>A value indicating whether the data was successfully fetched</returns>
        private bool FetchAndMerge()
        {
            Metric.Context("CACHE").Meter($"NamedValueCache-{_cacheName}->FetchAndMerge", Unit.Calls);
            EntityList<NamedValueDTO> record;
            try
            {
                record = _dataProvider.GetDataAsync().GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                ExecutionLog.Error("An exception occurred while fetching named values", ex.InnerException ?? ex);
                return false;
            }

            foreach (var item in record.Items)
            {
                _namedValues[item.Id] = item.Description;
            }
            CacheLog.Info($"{_cacheName}: {record.Items.Count()} items retrieved.");
            return true;
        }

        /// <summary>
        /// Determines whether specified id is present int the cache
        /// </summary>
        /// <param name="id">The id to be tested</param>
        /// <returns>True if the value is defined in the cache; False otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsValueDefined(int id)
        {
            lock (_lock)
            {
                if (!_dataFetched)
                {
                    _dataFetched = FetchAndMerge();
                }
            }
            return _namedValues.ContainsKey(id);
        }

        /// <summary>
        /// Gets the <see cref="INamedValue" /> specified by it's id
        /// </summary>
        /// <param name="id">The id of the <see cref="INamedValue" /> to retrieve</param>
        /// <returns>The specified <see cref="INamedValue" /></returns>
        public INamedValue GetNamedValue(int id)
        {
            lock (_lock)
            {
                if (!_dataFetched)
                {
                    _dataFetched = FetchAndMerge();
                }
            }
            string description;
            if (_namedValues.TryGetValue(id, out description))
            {
                return new NamedValue(id, description);
            }

            if (_exceptionStrategy == ExceptionHandlingStrategy.THROW)
            {
                throw new ArgumentOutOfRangeException($"Cache item missing for id={id}.");
            }
            return new NamedValue(id);
        }
    }
}
