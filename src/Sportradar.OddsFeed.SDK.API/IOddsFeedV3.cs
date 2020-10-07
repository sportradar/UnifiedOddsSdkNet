/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeedV3 : IOddsFeedV2
    {
        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
    }
}
