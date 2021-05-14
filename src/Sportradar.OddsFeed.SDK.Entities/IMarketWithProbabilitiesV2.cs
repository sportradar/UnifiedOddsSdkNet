/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by classes providing probability information for betting markets
    /// </summary>
    public interface IMarketWithProbabilitiesV2 : IMarketWithProbabilitiesV1
    {
        /// <summary>
        /// Gets the market metadata which contains the additional market information
        /// </summary>
        /// <value>The market metadata which contains the additional market information</value>
        IMarketMetadata MarketMetadata { get; }
    }
}