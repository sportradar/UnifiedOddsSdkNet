/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SportEventSummaryDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class SportEventSummaryMapperFactory : ISingleTypeMapperFactory<RestMessage, SportEventSummaryDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{SportEventSummaryDTO}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{SportEventSummaryDTO}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{SportEventSummaryDTO}" /> instance</returns>
        public ISingleTypeMapper<SportEventSummaryDTO> CreateMapper(RestMessage data)
        {
            if (data is matchSummaryEndpoint match)
            {
                return new SportEventSummaryMapper(match);
            }

            if (data is stageSummaryEndpoint stage)
            {
                return new SportEventSummaryMapper(stage);
            }

            if (data is tournamentInfoEndpoint tour)
            {
                return new SportEventSummaryMapper(tour);
            }

            throw new ArgumentException($"Unknown data type. Type={data.GetType().Name}", nameof(data));
        }
    }
}
