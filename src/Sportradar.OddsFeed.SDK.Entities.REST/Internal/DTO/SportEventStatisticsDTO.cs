/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for sport event status statistics. The status can be receiver through messages or fetched from the API
    /// </summary>
    public class SportEventStatisticsDTO
    {
        /// <summary>
        /// Total statistics
        /// </summary>
        public IEnumerable<TeamStatisticsDTO> TotalStatisticsDTOs { get; internal set; }

        /// <summary>
        /// Period statistics
        /// </summary>
        public IEnumerable<PeriodStatisticsDTO> PeriodStatisticsDTOs { get; internal set; }
        
        /// <summary>
        /// Constructor for statistics from feed 
        /// </summary>
        /// <param name="record"></param>
        public SportEventStatisticsDTO(statisticsType record)
        {
            Guard.Argument(record, nameof(record)).NotNull();

            var totalStatisticsDTOs = new List<TeamStatisticsDTO>();
            totalStatisticsDTOs.Add(new TeamStatisticsDTO(
                null,
                null,
                HomeAway.Home,
                record.yellow_cards.home,
                record.red_cards.home,
                record.yellow_red_cards.home,
                record.corners.home,
                record.green_cards.home
            ));
            totalStatisticsDTOs.Add(new TeamStatisticsDTO(
                null,
                null,
                HomeAway.Away,
                record.yellow_cards.away,
                record.red_cards.away,
                record.yellow_red_cards.away,
                record.corners.away,
                record.green_cards.away
            ));
            TotalStatisticsDTOs = totalStatisticsDTOs;

            PeriodStatisticsDTOs = null;
        }
        
        /// <summary>
        /// Constructor for statistics from API 
        /// </summary>
        /// <param name="statistics">Event statistics</param>
        /// <param name="homeAwayCompetitors"></param>
        public SportEventStatisticsDTO(matchStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(statistics, nameof(statistics)).NotNull();

            var teamStats = new List<TeamStatisticsDTO>();
            if (statistics.totals != null && statistics.totals.Any())
            {
                // can here be more then 1 sub-array? 
                foreach (var total in statistics.totals)
                {
                    foreach (var teamStatistics in total)
                    {
                        teamStats.Add(new TeamStatisticsDTO(teamStatistics, homeAwayCompetitors));
                    }
                }

                TotalStatisticsDTOs = teamStats;
            }

            if (statistics.periods != null)
            {
                var periodStats = new List<PeriodStatisticsDTO>();
                foreach (var period in statistics.periods)
                {
                    periodStats.Add(new PeriodStatisticsDTO(period, homeAwayCompetitors));
                }
                PeriodStatisticsDTOs = periodStats;
            }
        }
    }
}
