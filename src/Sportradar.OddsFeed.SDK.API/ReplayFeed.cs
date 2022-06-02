﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.Practices.Unity;
using Sportradar.OddsFeed.SDK.API.Internal;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// A <see cref="IOddsFeed"/> implementation acting as an entry point to the odds feed Replay Service for doing integration tests against played matches that are older than 48 hours
    /// </summary>
    public class ReplayFeed : Feed
    {
        /// <summary>
        /// The replay manager for interaction with xReplay Server
        /// </summary>
        public IReplayManagerV1 ReplayManager
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<IReplayManagerV1>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayFeed"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfiguration" /> instance representing feed configuration.</param>
        public ReplayFeed(IOddsFeedConfiguration config)
            : base(config, true)
        {
        }

        /// <inheritdoc />
        protected override void InitFeed()
        {
            if (FeedInitialized)
            {
                return;
            }
            base.InitFeed();
            ((ProducerManager)ProducerManager).SetIgnoreRecovery(0);
        }
    }
}
