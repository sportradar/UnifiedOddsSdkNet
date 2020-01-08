/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents an odds for an outcome (selection)
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="IOutcomeOdds"/> in next major version scheduled for January 2019</remarks>
    public interface IOutcomeOddsV2 : IOutcomeOddsV1
    {
        /// <summary>
        /// Additional probability attributes for markets which potentially will be (partly) refunded
        /// </summary>
        IAdditionalProbabilities AdditionalProbabilities { get; }
    }
}