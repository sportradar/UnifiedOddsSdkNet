/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes capable of interacting with Replay Server
    /// </summary>
    public interface IReplayManagerV2 : IReplayManagerV1
    {
        /// <summary>
        /// Gets list of replay events in queue.
        /// </summary>
        /// <returns>Returns a list of replay events</returns>
        IEnumerable<IReplayEvent> GetReplayEventsInQueue();
    }
}
