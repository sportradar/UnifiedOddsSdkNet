/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
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
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the group
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{ICompetitorCI}"/> representing group competitors
        /// </summary>
        public IEnumerable<URN> CompetitorsIds { get; private set; }

        /// <summary>
        /// The competitors references
        /// </summary>
        public IDictionary<URN, ReferenceIdCI> CompetitorsReferences { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal GroupCI(GroupDTO group, CultureInfo culture)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();
            
            Merge(group, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableGroupCI"/> containing group information</param>
        internal GroupCI(ExportableGroupCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }
            
            Id = exportable.Id;
            Name = exportable.Name;
            try
            {
                CompetitorsIds = exportable.Competitors?.Select(URN.Parse);
                if (!exportable.CompetitorsReferences.IsNullOrEmpty())
                {
                    CompetitorsReferences = new Dictionary<URN, ReferenceIdCI>();
                    foreach (var competitorsReference in exportable.CompetitorsReferences)
                    { 
                        var referenceIds = new Dictionary<string, string>();
                        var refs = competitorsReference.Value.Split(',');
                        foreach (var r in refs)
                        {
                            var refKeyValue = r.Split('=');
                            referenceIds.Add(refKeyValue[0], refKeyValue[1]);
                        }
                        CompetitorsReferences.Add(URN.Parse(competitorsReference.Key), new ReferenceIdCI(referenceIds));
                    }
                }
            }
            catch (Exception e)
            {
                SdkLoggerFactory.GetLoggerForExecution(typeof(GroupCI)).Error("Importing GroupCI", e);
            }
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

            if(string.IsNullOrEmpty(Id) || !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(group.Id))
            {
                Id = group.Id;
            }
            if(string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(group.Name))
            {
                Name = group.Name;
            }

            if (group.Competitors != null && group.Competitors.Any())
            {
                var tempCompetitors = CompetitorsIds == null ? new List<URN>() : new List<URN>(CompetitorsIds);
                var tempCompetitorsReferences = CompetitorsReferences == null ? new Dictionary<URN, ReferenceIdCI>() : new Dictionary<URN, ReferenceIdCI>(CompetitorsReferences);
                
                if (tempCompetitors.Count > 0 && tempCompetitors.Count != group.Competitors.Count())
                {
                    tempCompetitors.Clear();
                }

                if (!group.Competitors.All(a => tempCompetitors.Contains(a.Id)))
                {
                    tempCompetitors.Clear();
                }

                foreach (var competitor in group.Competitors)
                {
                    var tempCompetitor = tempCompetitors.FirstOrDefault(c => c.Equals(competitor.Id));
                    if (tempCompetitor == null)
                    {
                        tempCompetitors.Add(competitor.Id);
                        tempCompetitorsReferences.Add(competitor.Id, new ReferenceIdCI(competitor.ReferenceIds));
                    }
                }

                CompetitorsIds = new ReadOnlyCollection<URN>(tempCompetitors);
                CompetitorsReferences = new ReadOnlyDictionary<URN, ReferenceIdCI>(tempCompetitorsReferences);
            }
            else
            {
                CompetitorsIds = null;
                CompetitorsReferences = null;
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableGroupCI> ExportAsync()
        {
            var cr = new Dictionary<string, string>();
            if (!CompetitorsReferences.IsNullOrEmpty())
            {
                foreach (var competitorsReference in CompetitorsReferences)
                {
                    try
                    {
                        if (!competitorsReference.Value.ReferenceIds.IsNullOrEmpty())
                        {
                            var refs = string.Join(",", competitorsReference.Value.ReferenceIds.Select(s=> $"{s.Key}={s.Value}"));
                            cr.Add(competitorsReference.Key.ToString(), refs);
                        }
                    }
                    catch (Exception e)
                    {
                        SdkLoggerFactory.GetLoggerForExecution(typeof(GroupCI)).Error("Exporting GroupCI", e);
                    }
                }
            }

            return Task.FromResult(new ExportableGroupCI
            {
                Id = Id,
                Name = Name,
                Competitors = CompetitorsIds?.Select(s=>s.ToString()).ToList(),
                CompetitorsReferences = cr.IsNullOrEmpty() ? null : cr
            });
        }
    }
}
