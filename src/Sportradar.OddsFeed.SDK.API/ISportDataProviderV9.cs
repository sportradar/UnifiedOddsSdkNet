/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProviderV9 : ISportDataProviderV8
    {
        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of available lotteries</returns>
        Task<IEnumerable<ILottery>> GetLotteriesAsync(CultureInfo culture = null);
    }
}
