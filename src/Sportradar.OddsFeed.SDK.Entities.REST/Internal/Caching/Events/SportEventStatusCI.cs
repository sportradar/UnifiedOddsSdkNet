/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.CodeDom;
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
        public EventStatus Status => FeedDTO?.Status ?? SapiDTO?.Status ?? EventStatus.Unknown;

        /// <summary>
        /// Gets a value indicating whether a data journalist is present on the associated sport event, or a
        /// null reference if the information is not available
        /// </summary>
        public int? IsReported => FeedDTO?.IsReported ?? SapiDTO?.IsReported;

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        public decimal? HomeScore => FeedDTO?.HomeScore ?? SapiDTO?.HomeScore;

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        public decimal? AwayScore => FeedDTO?.AwayScore ?? SapiDTO?.AwayScore;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        public IReadOnlyDictionary<string, object> Properties => FeedDTO?.Properties ?? SapiDTO?.Properties;

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        public int MatchStatusId => FeedDTO?.MatchStatusId ?? SapiDTO?.MatchStatusId ?? 0;

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier</value>
        public URN WinnerId => FeedDTO?.WinnerId ?? SapiDTO?.WinnerId;

        /// <summary>
        /// Gets the reporting status
        /// </summary>
        /// <value>The reporting status</value>
        public ReportingStatus ReportingStatus => FeedDTO?.ReportingStatus ?? SapiDTO?.ReportingStatus ?? ReportingStatus.Unknown;

        /// <summary>
        /// Gets the period scores
        /// </summary>
        public IEnumerable<PeriodScoreDTO> PeriodScores => FeedDTO?.PeriodScores ?? SapiDTO?.PeriodScores;

        /// <summary>
        /// Gets the event clock
        /// </summary>
        /// <value>The event clock</value>
        public EventClockDTO EventClock => FeedDTO?.EventClock ?? SapiDTO?.EventClock;

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        public IEnumerable<EventResultDTO> EventResults => FeedDTO?.EventResults ?? SapiDTO?.EventResults;

        /// <summary>
        /// Gets the sport event statistics
        /// </summary>
        /// <value>The sport event statistics</value>
        public SportEventStatisticsDTO SportEventStatistics => FeedDTO?.SportEventStatistics ?? SapiDTO?.SportEventStatistics;

        /// <summary>
        /// Gets the indicator for competitors if there are home or away
        /// </summary>
        /// <value>The indicator for competitors if there are home or away</value>
        public IDictionary<HomeAway, URN> HomeAwayCompetitors => FeedDTO?._homeAwayCompetitors ?? SapiDTO?._homeAwayCompetitors;

        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? HomePenaltyScore => FeedDTO?.HomePenaltyScore ?? SapiDTO?.HomePenaltyScore;

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? AwayPenaltyScore => FeedDTO?.AwayPenaltyScore ?? SapiDTO?.AwayPenaltyScore;

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

        internal SportEventStatusDTO FeedDTO;
        internal SportEventStatusDTO SapiDTO;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusCI"/> class
        /// </summary>
        /// <param name="feedDto">The <see cref="SportEventStatusDTO"/> received from the feed</param>
        /// <param name="sapiDto">The <see cref="SportEventStatusDTO"/> received from the Sports API</param>
        public SportEventStatusCI(SportEventStatusDTO feedDto, SportEventStatusDTO sapiDto)
        {
            if (feedDto == null && sapiDto == null)
                throw new ArgumentNullException(nameof(feedDto));

            FeedDTO = feedDto;
            SapiDTO = sapiDto;
        }
    }
}