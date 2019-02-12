/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Internal.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for tournament season
    /// </summary>
    public class TournamentSeasonsDTO
    {
        /// <summary>
        /// Gets the tournament
        /// </summary>
        /// <value>The tournament</value>
        public TournamentInfoDTO Tournament { get; }

        /// <summary>
        /// Gets the seasons
        /// </summary>
        /// <value>The seasons</value>
        public IEnumerable<SeasonDTO> Seasons { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentSeasonsDTO"/> class
        /// </summary>
        /// <param name="item">The item</param>
        internal TournamentSeasonsDTO(tournamentSeasons item)
        {
            Contract.Requires(item != null);

            Tournament = new TournamentInfoDTO(item.tournament);

            if (item.seasons != null && item.seasons.Any())
            {
                Seasons = item.seasons.Select(s => new SeasonDTO(s));
            }
        }
    }
}
