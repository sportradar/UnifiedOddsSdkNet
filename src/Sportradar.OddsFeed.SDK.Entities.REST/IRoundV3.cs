/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing basic tournament round information
    /// </summary>
    public interface IRoundV3 : IRoundV2
    {
        /// <summary>
        /// A betradar name
        /// </summary>
        string BetradarName { get; }
    }
}