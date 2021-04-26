/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProviderV5 : ISportDataProviderV4
    {
        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <param name="cacheType">Specifies what type of cache items will be exported</param>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        Task<IEnumerable<ExportableCI>> CacheExportAsync(CacheType cacheType);

        /// <summary>
        /// Imports provided items into caches
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableCI"/> containing the items to be imported</param>
        Task CacheImportAsync(IEnumerable<ExportableCI> items);
    }
}
