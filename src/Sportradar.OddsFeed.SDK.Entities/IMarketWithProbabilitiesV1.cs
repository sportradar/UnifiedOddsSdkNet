/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by classes providing probability information for betting markets
    /// </summary>
    public interface IMarketWithProbabilitiesV1 : IMarketWithProbabilities
    {
        /// <summary>
        /// Gets a <see cref="CashoutStatus"/> enum member specifying the availability of cashout, or a null reference
        /// </summary>
        CashoutStatus? CashoutStatus { get; }
    }
}