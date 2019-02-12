/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A implementation of cache item for Group
    /// </summary>
    public class GroupCI
    {
        /// <summary>
        /// Gets the name of the group
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{ICompetitorCI}"/> representing group competitors
        /// </summary>
        public IEnumerable<CompetitorCI> Competitors { get; private set; }

        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class.
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        internal GroupCI(GroupDTO group, CultureInfo culture, IDataRouterManager dataRouterManager)
        {
            Contract.Requires(group != null);
            Contract.Requires(culture != null);

            _dataRouterManager = dataRouterManager;
            Name = group.Name;
            Competitors = group.Competitors != null
                ? new ReadOnlyCollection<CompetitorCI>(group.Competitors.Select(c => new CompetitorCI(c, culture, _dataRouterManager)).ToList())
                : null;
        }

        /// <summary>
        /// Merges the provided group information with the information held by the current instance
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal void Merge(GroupDTO group, CultureInfo culture)
        {
            Contract.Requires(group != null);
            Contract.Requires(culture != null);

            var tempCompetitors = new List<CompetitorCI>(Competitors);

            foreach (var competitor in group.Competitors)
            {
                var tempCompetitor = tempCompetitors.FirstOrDefault(c => c.Id.Equals(competitor.Id));
                if (tempCompetitor == null)
                {
                    tempCompetitors.Add(new CompetitorCI(competitor, culture, _dataRouterManager));
                }
                else
                {
                    tempCompetitor.Merge(competitor, culture);
                }
            }
            Competitors = new ReadOnlyCollection<CompetitorCI>(tempCompetitors);
        }
    }
}
