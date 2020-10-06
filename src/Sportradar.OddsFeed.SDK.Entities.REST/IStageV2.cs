/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines methods implemented by classes representing sport events of stage type
    /// </summary>
    public interface IStageV2 : IStageV1, ICompetitionV2
    {
        /// <summary>
        /// Asynchronously get the type of the stage
        /// </summary>
        /// <returns>The type of the stage</returns>
        new Task<StageType?> GetStageTypeAsync();

        /// <summary>
        /// Asynchronously get the type of the stage
        /// </summary>
        /// <returns>The type of the stage</returns>
        Task<SportEventType?> GetEventTypeAsync();

        /// <summary>
        /// Asynchronously gets a list of additional ids of the parent stages of the current instance or a null reference if the represented stage does not have the parent stages
        /// </summary>
        /// <returns>A <see cref="Task{StageCI}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<IStage>> GetAdditionalParentStagesAsync();
    }
}
