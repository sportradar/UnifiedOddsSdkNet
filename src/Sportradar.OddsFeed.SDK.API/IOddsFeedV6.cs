/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeedV6 : IOddsFeedV5
    {
        /// <summary>
        /// Returns an indicator if the feed instance is opened or not
        /// </summary>
        bool IsOpen();
    }
}
