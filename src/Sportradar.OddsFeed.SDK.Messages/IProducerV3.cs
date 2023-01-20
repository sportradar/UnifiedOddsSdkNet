/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Defines a contract for producer which use the feed to dispatch messages
    /// </summary>
    public interface IProducerV3 : IProducerV2
    {
        /// <summary>
        /// Gets the scope of the producer
        /// </summary>
        /// <value>The scope</value>
        IReadOnlyCollection<string> Scope { get; }
    }
}