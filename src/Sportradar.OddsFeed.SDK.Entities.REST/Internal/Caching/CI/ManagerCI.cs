/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

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
            Contract.Requires(item != null);

            if (Nationality == null)
            {
                Nationality = new Dictionary<CultureInfo, string>();
            }

            Nationality[culture] = item.Nationality;
            CountryCode = item.CountryCode;
        }

        /// <summary>
        /// Merges the specified item
        /// </summary>
        /// <param name="item">The item with the manager info</param>
        /// <param name="culture">The culture.</param>
        public void Merge(ManagerDTO item, CultureInfo culture)
        {
            Contract.Requires(item != null);

            base.Merge(item, culture);

            Nationality[culture] = item.Nationality;
        }
    }
}
