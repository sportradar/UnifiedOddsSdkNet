/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing product information
    /// </summary>
    public interface IProductInfoV1 : IProductInfo
    {
        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in LiveMatchTracker solution
        /// </summary>
        bool IsInLiveMatchTracker { get; }
    }
}
