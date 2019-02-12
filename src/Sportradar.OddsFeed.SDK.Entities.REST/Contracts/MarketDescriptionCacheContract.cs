/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Contracts
{
    [ContractClassFor(typeof(IMarketDescriptionCache))]
    abstract class MarketDescriptionCacheContract : IMarketDescriptionCache
    {
        public Task<IMarketDescription> GetMarketDescriptionAsync(int marketId, string variant, IEnumerable<CultureInfo> cultures)
        {
            Contract.Requires(cultures != null && cultures.Any());
            Contract.Ensures(Contract.Result<Task<IMarketDescription>>() != null);

            return Contract.Result<Task<IMarketDescription>>();
        }
    }
}
