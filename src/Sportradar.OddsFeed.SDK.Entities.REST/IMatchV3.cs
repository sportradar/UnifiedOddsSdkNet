/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport events of match type
    /// </summary>
    /// <remarks>Added May 2021</remarks>
    public interface IMatchV3 : IMatchV2
    {
        /// <summary>
        /// Asynchronously gets the associated event timeline for single culture
        /// </summary>
        /// <param name="culture">The languages to which the returned instance should be translated</param>
        /// <remarks>Recommended to be used when only <see cref="IEventTimeline"/> is needed for this <see cref="IMatch"/></remarks>
        /// <returns>A <see cref="Task{IEventTimeline}"/> representing the retrieval operation</returns>
        Task<IEventTimeline> GetEventTimelineAsync(CultureInfo culture);
    }
}
