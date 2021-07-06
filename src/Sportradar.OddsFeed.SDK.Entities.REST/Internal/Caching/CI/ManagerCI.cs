﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// An implementation of the Manager cache item
    /// </summary>
    public class ManagerCI : CacheItem
    {
        /// <summary>
        /// Gets a <see cref="IDictionary{CultureInfo, String}"/> containing translated nationality of the item
        /// </summary>
        public IDictionary<CultureInfo, string> Nationality { get; }

        /// <summary>
        /// Gets the country code of the manager
        /// </summary>
        /// <value>The country code of the manager</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerCI"/> class
        /// </summary>
        /// <param name="item">The dto with manager info</param>
        /// <param name="culture">The culture</param>
        public ManagerCI(ManagerDTO item, CultureInfo culture)
            : base(item.Id, item.Name, culture)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            if (Nationality == null)
            {
                Nationality = new Dictionary<CultureInfo, string>();
            }

            Nationality[culture] = item.Nationality;
            CountryCode = item.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerCI"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableManagerCI"/> with manager info</param>
        public ManagerCI(ExportableManagerCI exportable) 
            : base(URN.Parse(exportable.Id), new Dictionary<CultureInfo, string>(exportable.Name))
        {
            Nationality = exportable.Nationality != null
                ? new Dictionary<CultureInfo, string>(exportable.Nationality)
                : null;
            CountryCode = exportable.CountryCode;
        }

        /// <summary>
        /// Merges the specified item
        /// </summary>
        /// <param name="item">The item with the manager info</param>
        /// <param name="culture">The culture.</param>
        public void Merge(ManagerDTO item, CultureInfo culture)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            base.Merge(item, culture);

            Nationality[culture] = item.Nationality;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableManagerCI"/> instance containing all relevant properties</returns>
        public Task<ExportableManagerCI> ExportAsync()
        {
            return Task.FromResult(new ExportableManagerCI
            {
                Id = Id.ToString(),
                Name = new Dictionary<CultureInfo, string>(Name),
                Nationality = Nationality.IsNullOrEmpty() ? null : new Dictionary<CultureInfo, string>(Nationality),
                CountryCode = CountryCode
            });
        }
    }
}
