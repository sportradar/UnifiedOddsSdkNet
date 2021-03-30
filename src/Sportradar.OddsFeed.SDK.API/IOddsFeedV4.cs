/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeedV4 : IOddsFeedV3
    {
        /// <summary>
        /// Gets a <see cref="IEventChangeManager"/> instance used to automatically receive fixture and result changes
        /// </summary>
        IEventChangeManager EventChangeManager { get; }
    }
}
