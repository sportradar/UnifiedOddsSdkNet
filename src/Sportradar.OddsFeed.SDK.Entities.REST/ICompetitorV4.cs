/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitorV4 : ICompetitorV3
    {
        /// <summary>
        /// Gets the state
        /// </summary>
        /// <value>The state</value>
        string State { get; }
    }
}
