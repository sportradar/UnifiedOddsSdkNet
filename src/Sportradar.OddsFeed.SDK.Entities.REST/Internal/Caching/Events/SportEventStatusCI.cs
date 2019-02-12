/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class SportEventStatusCI
    /// </summary>
    /// <seealso cref="ISportEventStatusCI" />
    public class SportEventStatusCI : ISportEventStatusCI
    {
        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        public EventStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether a data journalist is present on the associated sport event, or a
        /// null reference if the information is not available
        /// </summary>
        public int? IsReported { get; }

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        public decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        public decimal? AwayScore { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        public IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        public int MatchStatusId { get; }

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier</value>
        public URN WinnerId { get; }

        /// <summary>
        /// Gets the reporting status
        /// </summary>
        /// <value>The reporting status</value>
        public ReportingStatus ReportingStatus { get; }

        /// <summary>
        /// Gets the period scores
        /// </summary>
        public IEnumerable<PeriodScoreDTO> PeriodScores { get; }

        /// <summary>
        /// Gets the event clock
        /// </summary>
        /// <value>The event clock</value>
        public EventClockDTO EventClock { get; }

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        public IEnumerable<EventResultDTO> EventResults { get; }

        /// <summary>
        /// Gets the sport event statistics
        /// </summary>
        /// <value>The sport event statistics</value>
        public SportEventStatisticsDTO SportEventStatistics { get; }

        /// <summary>
        /// Gets the indicator for competitors if there are home or away
        /// </summary>
        /// <value>The indicator for competitors if there are home or away</value>
        public IDictionary<HomeAway, URN> HomeAwayCompetitors { get; }

        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? HomePenaltyScore { get; }

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? AwayPenaltyScore { get; }

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object"/> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        public object GetPropertyValue(string propertyName)
        {
            return Properties[propertyName];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusCI"/> class
        /// </summary>
        /// <param name="dto">The <see cref="SportEventStatusDTO"/></param>
        public SportEventStatusCI(SportEventStatusDTO dto)
        {
            Contract.Requires(dto != null);

            Status = dto.Status;
            IsReported = dto.IsReported;
            HomeScore = dto.HomeScore;
            AwayScore = dto.AwayScore;
            Properties = dto.Properties;
            MatchStatusId = dto.MatchStatusId;
            WinnerId = dto.WinnerId;
            ReportingStatus = dto.ReportingStatus;
            PeriodScores = dto.PeriodScores;
            EventClock = dto.EventClock;
            EventResults = dto.EventResults;
            SportEventStatistics = dto.SportEventStatistics;
            HomeAwayCompetitors = dto._homeAwayCompetitors;
            HomePenaltyScore = dto.HomePenaltyScore;
            AwayPenaltyScore = dto.AwayPenaltyScore;
        }
    }
}