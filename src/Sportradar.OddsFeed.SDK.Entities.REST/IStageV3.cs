/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines methods implemented by classes representing sport events of stage type
    /// </summary>
    public interface IStageV3 : IStageV2
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IStageStatus"/> containing information about the progress of the stage
        /// </summary>
        /// <returns>A <see cref="Task{IStageStatus}"/> containing information about the progress of the stage</returns>
        new Task<IStageStatus> GetStatusAsync();
    }
}
