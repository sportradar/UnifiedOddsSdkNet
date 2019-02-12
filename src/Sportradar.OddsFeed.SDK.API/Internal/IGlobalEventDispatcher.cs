/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to dispatch global SDK events
    /// </summary>
    [ContractClass(typeof(GlobalEventDispatcherContract))]
    internal interface IGlobalEventDispatcher
    {
        /// <summary>
        /// Dispatches the <see cref="IProducerStatusChange"/> message
        /// </summary>
        /// <param name="producerStatusChange">The <see cref="IProducerStatusChange"/> instance to be dispatched</param>
        void DispatchProducerDown(IProducerStatusChange producerStatusChange);

        /// <summary>
        /// Dispatches the <see cref="IProducerStatusChange"/> message
        /// </summary>
        /// <param name="producerStatusChange">The <see cref="IProducerStatusChange"/> instance to be dispatched</param>
        void DispatchProducerUp(IProducerStatusChange producerStatusChange);

        /// <summary>
        /// Dispatches the information that the connection to the feed was lost
        /// </summary>
        void DispatchDisconnected();
    }
}
