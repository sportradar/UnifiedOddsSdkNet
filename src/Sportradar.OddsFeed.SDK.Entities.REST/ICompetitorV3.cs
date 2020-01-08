/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitorV3 : ICompetitorV2
    {
        /// <summary>
        /// Gets the age group
        /// </summary>
        /// <value>The age group</value>
        string AgeGroup { get; }
    }
}
