using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal static class MarketHelper
    {
        /// <summary>
        /// List of <see cref="ISpecifier"/> to string
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>System.String.</returns>
        public static string SpecifiersToString(IEnumerable<ISpecifier> specifiers)
        {
            if (specifiers == null)
            {
                return string.Empty;
            }
            var tmp = string.Join("|", specifiers);
            return tmp;
        }

        /// <summary>
        /// Get the list of specifier names
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>System.String.</returns>
        public static string SpecifiersKeysToString(IEnumerable<ISpecifier> specifiers)
        {
            if (specifiers == null)
            {
                return string.Empty;
            }

            var tmp = string.Join(",", specifiers.Select(s => s.Name));
            return tmp;
        }
    }
}
