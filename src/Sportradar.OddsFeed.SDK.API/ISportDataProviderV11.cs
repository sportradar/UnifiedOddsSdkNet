/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProviderV11 : ISportDataProviderV10
    {
        /// <summary>
        /// Get the associated event timeline for single culture
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <returns>The event timeline or empty if not found</returns>
        Task<IEnumerable<ITimelineEvent>> GetTimelineEventsAsync(URN id, CultureInfo culture = null);
    }
}
