/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing tournament information
    /// </summary>
    /// <seealso cref="ILongTermEvent" />
    public interface IBasicTournamentV2 : IBasicTournamentV1
    {
        /// <summary>
        /// Gets the list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule
        /// </summary>
        /// <returns>The list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule</returns>
        Task<IEnumerable<ISportEvent>> GetScheduleAsync();
    }
}
