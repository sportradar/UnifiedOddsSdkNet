/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing team statistics
    /// </summary>
    public interface ITeamStatisticsV2 : ITeamStatisticsV1
    {
        /// <summary>
        /// Gets the total count of green cards
        /// </summary>
        /// <value>The total count of green cards</value>
        int? GreenCards { get; }
    }
}
