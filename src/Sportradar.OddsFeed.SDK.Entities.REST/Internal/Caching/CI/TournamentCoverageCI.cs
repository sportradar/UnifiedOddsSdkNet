/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for TournamentCoverage
    /// </summary>
    public class TournamentCoverageCI
    {
        /// <summary>
        /// Gets a value indicating whether [live coverage].
        /// </summary>
        /// <value><c>true</c> if [live coverage]; otherwise, <c>false</c>.</value>
        public bool LiveCoverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverageCI"/> class.
        /// </summary>
        /// <param name="tournamentCoverage">The tournament coverage.</param>
        internal TournamentCoverageCI(TournamentCoverageDTO tournamentCoverage)
        {
            Contract.Requires(tournamentCoverage != null);

            LiveCoverage = tournamentCoverage.LiveCoverage;
        }
    }
}
