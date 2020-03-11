﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A implementation of cache item for Group
    /// </summary>
    public class GroupCI
    {
        /// <summary>
        /// Gets the id of the group
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the group
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{ICompetitorCI}"/> representing group competitors
        /// </summary>
        public IEnumerable<CompetitorCI> Competitors { get; private set; }

        /// <summary>
        /// The competitors references
        /// </summary>
        public IDictionary<URN, ReferenceIdCI> CompetitorsReferences { get; private set; }

        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        internal GroupCI(GroupDTO group, CultureInfo culture, IDataRouterManager dataRouterManager)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _dataRouterManager = dataRouterManager;
            Id = group.Id;
            Name = group.Name;
            Competitors = group.Competitors != null
                ? new ReadOnlyCollection<CompetitorCI>(group.Competitors.Select(c => new CompetitorCI(c, culture, _dataRouterManager)).ToList())
                : null;
            CompetitorsReferences = group.Competitors != null
                ? new ReadOnlyDictionary<URN, ReferenceIdCI>(group.Competitors.ToDictionary(c => c.Id, c => new ReferenceIdCI(c.ReferenceIds)))
                : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableGroupCI"/> containing group information</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        internal GroupCI(ExportableGroupCI exportable, IDataRouterManager dataRouterManager)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            _dataRouterManager = dataRouterManager;
            Id = exportable.Id;
            Name = exportable.Name;
            Competitors = exportable.Competitors?.Select(c => new CompetitorCI(c, dataRouterManager)).ToList();
            CompetitorsReferences = exportable.CompetitorsReferences?.ToDictionary(c => URN.Parse(c.Key), c => new ReferenceIdCI(c.Value));
        }

        /// <summary>
        /// Merges the provided group information with the information held by the current instance
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal void Merge(GroupDTO group, CultureInfo culture)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

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
            CompetitorsReferences =
                new ReadOnlyDictionary<URN, ReferenceIdCI>(tempCompetitors.ToDictionary(c => c.Id, c => c.ReferenceId));
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableGroupCI> ExportAsync()
        {
            var competitorsTask = Competitors?.Select(async c => await c.ExportAsync().ConfigureAwait(false) as ExportableCompetitorCI);

            return new ExportableGroupCI
            {
                Id = Id,
                Competitors = competitorsTask != null ? await Task.WhenAll(competitorsTask) : null,
                CompetitorsReferences = CompetitorsReferences?.ToDictionary(c => c.Key.ToString(), c => c.Value.ReferenceIds.ToDictionary(r => r.Key, r => r.Value)),
                Name = Name
            };
        }
    }
}
