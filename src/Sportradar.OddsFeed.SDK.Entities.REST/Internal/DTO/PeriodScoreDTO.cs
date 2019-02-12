/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.Internal.Feed;
using Sportradar.OddsFeed.SDK.Messages.Internal.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Class PeriodScoreDTO
    /// </summary>
    public class PeriodScoreDTO
    {
        /// <summary>
        /// Gets the home score
        /// </summary>
        /// <value>The home score</value>
        public decimal HomeScore { get; }

        /// <summary>
        /// Gets the away score
        /// </summary>
        /// <value>The away score</value>
        public decimal AwayScore { get; }

        /// <summary>
        /// Gets the period number
        /// </summary>
        /// <value>The period number</value>
        public int? PeriodNumber { get; }

        /// <summary>
        /// Gets the match status code
        /// </summary>
        /// <value>The match status code</value>
        public int? MatchStatusCode { get; }

        /// <summary>
        /// Gets the type
        /// </summary>
        /// <value>The type</value>
        public PeriodType? Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodScoreDTO"/> class
        /// </summary>
        /// <param name="periodScore">The period score</param>
        public PeriodScoreDTO(periodScoreType periodScore)
        {
            HomeScore = periodScore.home_score;
            AwayScore = periodScore.away_score;
            PeriodNumber = periodScore.number;
            MatchStatusCode = periodScore.match_status_code;
            Type = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodScoreDTO"/> class
        /// </summary>
        /// <param name="periodScore">The period score</param>
        public PeriodScoreDTO(periodScore periodScore)
        {
            HomeScore = (decimal)periodScore.home_score;
            AwayScore = (decimal)periodScore.away_score;
            PeriodNumber = periodScore.number;
            MatchStatusCode = null;
            Type = string.IsNullOrEmpty(periodScore.type) ? (PeriodType?)null : GetPeriodType(periodScore.type);
        }

        private static PeriodType GetPeriodType(string periodType)
        {
            switch (periodType.ToLower())
            {
                case "overtime":
                    return PeriodType.Overtime;
                case "penalties":
                    return PeriodType.Penalties;
                case "regular_period":
                    return PeriodType.RegularPeriod;
                default:
                    return PeriodType.Other;
            }
        }
    }
}
