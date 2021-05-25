/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing information about tournament schedule
    /// </summary>
    public interface ITournamentV2 : ITournamentV1
    {
        /// <summary>
        /// Gets the list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule
        /// </summary>
        /// <returns>The list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule</returns>
        Task<IEnumerable<ISportEvent>> GetScheduleAsync();
    }
}
