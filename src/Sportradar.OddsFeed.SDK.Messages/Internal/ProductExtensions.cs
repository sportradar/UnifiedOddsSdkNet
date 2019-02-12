using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Messages.Internal
{
    /// <summary>
    /// Class defining extension methods for <see cref="Product"/> enum
    /// </summary>
    public static class ProductExtensions
    {
        /// <summary>
        /// A <see cref="IEnumerable{Product}"/> containing all <see cref="Product"/> members
        /// </summary>
        private static IEnumerable<Product> _allProducts;

        /// <summary>
        /// Gets a <see cref="IEnumerable{Product}"/> containing all <see cref="Product"/> members
        /// </summary>
        /// <returns>The <see cref="IEnumerable{Product}"/> containing all <see cref="Product"/> members.</returns>
        public static IEnumerable<Product> AllProducts()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Product>>()!= null && Contract.Result<IEnumerable<Product>>().Any());
            return _allProducts ?? (_allProducts = Enum.GetValues(typeof(Product)).OfType<Product>().ToArray());
        }

        /// <summary>
        /// Gets the name of the provided enum member
        /// </summary>
        /// <param name="product">The <see cref="Product"/> enum member for which to get the name.</param>
        /// <returns>The name of the provided enum member.</returns>
        public static string GetName(this Product product)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
            return Enum.GetName(typeof(Product), product);
        }

        /// <summary>
        /// Gets the product code string used for REST requests
        /// </summary>
        /// <param name="product">The <see cref="Product"/> used to generate string</param>
        /// <exception cref="ArgumentException">The product code for specified product is not defined</exception>
        public static string GetProductCode(this Product product)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
            switch (product)
            {
                case Product.LIVE_ODDS:
                    return "liveodds";
                case Product.LCOO:
                    return "pre";
                case Product.PREMIUM_CRICKET:
                    return "premium_cricket";
                case Product.BETPAL:
                    return "betpal";
                default:
                    throw new ArgumentException($"Product code for Product={Enum.GetName(typeof(Product), product)} is not defined", nameof(product));
            }
        }

        /// <summary>
        /// Gets the maximum allowed duration of the recovery operation in seconds for the specified product
        /// </summary>
        /// <param name="product">A <see cref="Product"/> specifying the product.</param>
        /// <returns>The maximum allowed duration of the recovery operation in seconds for the specified product.</returns>
        public static int MaxRecoveryDurationSec(this Product product)
        {
            return product == Product.LCOO
                ? 1800
                : 900;
        }

        /// <summary>
        /// Determines whether the specified product is enabled on the feed. 
        /// </summary>
        /// <param name="product">The <see cref="Product"/> in question.</param>
        /// <returns>True if the specified product is enabled; False otherwise.</returns>
        public static bool IsEnabled(this Product product)
        {
            if (product == Product.LCOO || product == Product.LIVE_ODDS || product == Product.BETPAL || product == Product.PREMIUM_CRICKET)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> specifying the max age of after param when doing a after timestamp recovery
        /// </summary>
        /// <param name="product">The <see cref="Product"/> associated with the after timestamp recovery.</param>
        /// <returns>A <see cref="TimeSpan"/> specifying the max age of after param when doing a after timestamp recovery.</returns>
        public static TimeSpan MaxAfterAge(this Product product)
        {
            Contract.Ensures(Contract.Result<TimeSpan>() > TimeSpan.Zero);
            return TimeSpan.FromDays(3);
        }
    }
}
