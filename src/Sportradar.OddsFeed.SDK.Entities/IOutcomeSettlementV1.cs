/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represent settlement information for an outcome(market selection)
    /// </summary>
    public interface IOutcomeSettlementV1 : IOutcomeSettlement
    {
        /// <summary>
        /// Gets a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement"/> is winning - i.e. have the bets placed on this outcome winning or losing
        /// </summary>
        OutcomeResult OutcomeResult { get; }
    }
}
