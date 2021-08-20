/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Defines a contract for producer which use the feed to dispatch messages
    /// </summary>
    public interface IProducerV2 : IProducerV1
    {
        /// <summary>
        /// Gets the stateful recovery window in minutes.
        /// </summary>
        /// <value>The stateful recovery window in minutes.</value>
        int StatefulRecoveryWindow { get; }
    }
}