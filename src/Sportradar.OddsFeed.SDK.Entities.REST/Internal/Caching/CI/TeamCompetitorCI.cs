/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Team competitor cache item representation of <see cref="ITeamCompetitor"/>
    /// </summary>
    public class TeamCompetitorCI : CompetitorCI
    {
        /// <summary>
        /// Gets a qualifier additionally describing the competitor (e.g. home, away, ...)
        /// </summary>
        public string Qualifier { get; private set; }

        /// <summary>
        /// Gets the division
        /// </summary>
        /// <value>The division</value>
        public int? Division { get; private set; }

        /// <summary>
        /// Initializes new TeamCompetitorCI instance
        /// </summary>
        /// <param name="competitor">A <see cref="TeamCompetitorDTO"/> to be used to construct new instance</param>
        /// <param name="culture">A culture to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCI(TeamCompetitorDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor, culture, dataRouterManager)
        {
            Contract.Requires(competitor != null);
            Contract.Requires(culture != null);

            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes new TeamCompetitorCI instance
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDTO"/> to be used to construct new instance</param>
        /// <param name="culture">A culture to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCI(CompetitorDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor, culture, dataRouterManager)
        {
            Contract.Requires(competitor != null);
            Contract.Requires(culture != null);

            Merge(competitor, culture);
        }

        /// <summary>
        /// Merges the specified <see cref="TeamCompetitorDTO"/> into instance
        /// </summary>
        /// <param name="competitor">The <see cref="TeamCompetitorDTO"/> used for merge</param>
        /// <param name="culture">The culture of the input <see cref="TeamCompetitorDTO"/></param>
        internal void Merge(TeamCompetitorDTO competitor, CultureInfo culture)
        {
            Contract.Requires(competitor != null);
            Contract.Requires(culture != null);

            base.Merge(competitor, culture);
            Qualifier = competitor.Qualifier;
            Division = competitor.Division;
        }
    }
}
