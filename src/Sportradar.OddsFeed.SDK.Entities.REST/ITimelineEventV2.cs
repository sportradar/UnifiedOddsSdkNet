/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing timeline event
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="ITimelineEvent"/> in next major version scheduled for January 2019</remarks>
    public interface ITimelineEventV2 : ITimelineEventV1
    {
        /// <summary>
        /// Gets the player
        /// </summary>
        /// <value>The player</value>
        new IEventPlayer Player { get; }
    }
}
