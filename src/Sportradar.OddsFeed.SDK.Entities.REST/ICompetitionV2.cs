/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport events regardless to which sport they belong
    /// </summary>
    public interface ICompetitionV2 : ICompetitionV1, ISportEventV1
    {
        /// <summary>
        /// Asynchronously gets a liveOdds
        /// </summary>
        /// <returns>A liveOdds</returns>
        Task<string> GetLiveOddsAsync();

        /// <summary>
        /// Asynchronously get the type of the stage
        /// </summary>
        /// <returns>The type of the stage</returns>
        Task<SportEventType?> GetSportEventTypeAsync();
    }
}