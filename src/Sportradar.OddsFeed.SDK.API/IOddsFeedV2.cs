/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeedV2 : IOddsFeedV1
    {
        /// <summary>
        /// Gets a <see cref="ICustomBetManager"/> instance used to perform various custom bet operations
        /// </summary>
        ICustomBetManager CustomBetManager { get; }
    }
}
