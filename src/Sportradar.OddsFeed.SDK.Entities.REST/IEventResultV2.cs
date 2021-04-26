/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines methods used by classes that provide event result information
    /// </summary>
    public interface IEventResultV2 : IEventResultV1
    {
        /// <summary>
        /// Gets the distance
        /// </summary>
        /// <value>The distance</value>
        double? Distance { get; }

        /// <summary>
        /// Gets the competitor results
        /// </summary>
        /// <value>The results</value>
        IEnumerable<ICompetitorResult> CompetitorResults { get; }
    }
}
