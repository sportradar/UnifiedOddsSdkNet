/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeedV2 : IOddsFeedV1
    {
        /// <summary>
        /// Gets a <see cref="IMarketDescriptionManager"/> instance used to get info about available markets, and to get translations for markets and outcomes including outrights
        /// </summary>
        IMarketDescriptionManager MarketDescriptionManager { get; }
    }
}
