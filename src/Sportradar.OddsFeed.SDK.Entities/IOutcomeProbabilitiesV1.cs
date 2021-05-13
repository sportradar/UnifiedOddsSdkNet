/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents a selection with probabilities information
    /// </summary>
    /// <seealso cref="IOutcome" />
    public interface IOutcomeProbabilitiesV1 : IOutcomeProbabilities
    {
        /// <summary>
        /// Additional probability attributes for markets which potentially will be (partly) refunded
        /// </summary>
        IAdditionalProbabilities AdditionalProbabilities { get; }
    }
}