/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IGlobalEventDispatcher))]
    abstract class GlobalEventDispatcherContract : IGlobalEventDispatcher
    {
        public void DispatchDisconnected()
        {

        }

        public void DispatchProducerDown(IProducerStatusChange producerStatusChange)
        {
            Contract.Requires(producerStatusChange != null);
        }

        public void DispatchProducerUp(IProducerStatusChange producerStatusChange)
        {
            Contract.Requires(producerStatusChange != null);
        }
    }
}
