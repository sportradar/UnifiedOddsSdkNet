/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a outcome definition
    /// </summary>
    internal class OutcomeDefinition : IOutcomeDefinition
    {
        /// <summary>
        /// The associated market descriptor
        /// </summary>
        private readonly IMarketDescription _marketDescription;

        private readonly string _outcomeId;
        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing names in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _names = new Dictionary<CultureInfo, string>();

        /// <summary>
        /// Constructs a new market definition. The market definition represents additional market data which can be used for more advanced use cases
        /// </summary>
        /// <param name="marketDescription">The associated market descriptor</param>
        /// <param name="outcomeDescription">The associated outcome descriptor</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal OutcomeDefinition(IMarketDescription marketDescription, IOutcomeDescription outcomeDescription, IEnumerable<CultureInfo> cultures)
        {
            _marketDescription = marketDescription;

            if (outcomeDescription != null)
            {
                _outcomeId = outcomeDescription.Id;
                foreach (var culture in cultures)
                {
                    _names[culture] = outcomeDescription.GetName(culture);
                }
            }
        }

        /// <summary>
        /// Returns the unmodified market name template
        /// </summary>
        /// <param name="culture">The culture in which the name template should be provided</param>
        /// <returns>The unmodified market name template</returns>
        public string GetNameTemplate(CultureInfo culture)
        {
            return _names.ContainsKey(culture)
                       ? _names[culture]
                       : null;
        }
    }
}
