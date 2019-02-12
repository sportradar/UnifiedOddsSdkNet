/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Metrics;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Contracts
{
    [ContractClassFor(typeof(IHealthStatusProvider))]
    internal abstract class HealthStatusProviderContract : IHealthStatusProvider
    {
        public void RegisterHealthCheck()
        {
        }

        public HealthCheckResult StartHealthCheck()
        {
            return Contract.Result<HealthCheckResult>();
        }
    }
}
