/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for fixture delayed info
    /// </summary>
    public class DelayedInfoCI
    {
        /// <summary>
        /// Gets the identifier
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing descriptions in different languages
        /// </summary>
        public readonly IDictionary<CultureInfo, string> Descriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedInfoCI"/> class
        /// </summary>
        /// <param name="dto">The <see cref="DelayedInfoCI"/> used to create new instance</param>
        /// <param name="culture">The culture of the input <see cref="RoundDTO"/></param>
        internal DelayedInfoCI(DelayedInfoDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);
            Contract.Requires(culture != null);

            Descriptions = new Dictionary<CultureInfo, string>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Merges the specified <see cref="DelayedInfoCI"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="DelayedInfoCI"/> used fro merging</param>
        /// <param name="culture">The culture of the input <see cref="DelayedInfoCI"/></param>
        internal void Merge(DelayedInfoDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);
            Id = dto.Id;
            Descriptions[culture] = dto.Description;
        }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Name if exists, or null</returns>
        public string GetDescription(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            return Descriptions == null || !Descriptions.ContainsKey(culture)
                ? null
                : Descriptions[culture];
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false</returns>
        public virtual bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => Descriptions.ContainsKey(c));
        }
    }
}
