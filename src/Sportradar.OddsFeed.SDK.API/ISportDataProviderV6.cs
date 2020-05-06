/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProviderV6 : ISportDataProviderV5
    {
        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        Task<IEnumerable<IResultChange>> GetResultChangesAsync(CultureInfo culture = null);
    }
}
