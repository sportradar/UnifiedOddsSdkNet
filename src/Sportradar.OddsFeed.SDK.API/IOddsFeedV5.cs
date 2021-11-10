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
    public interface IOddsFeedV5 : IOddsFeedV4
    {
        /// <summary>
        /// Occurs when a recovery initiation completes
        /// </summary>
        event EventHandler<RecoveryInitiatedEventArgs> RecoveryInitiated;
    }
}
