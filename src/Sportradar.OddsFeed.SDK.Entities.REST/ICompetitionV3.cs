/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport events regardless to which sport they belong
    /// </summary>
    //TODO - remove GetCompetitorIdsAsync
    public interface ICompetitionV3 : ICompetitionV2
    {
        /// <summary>
        /// Asynchronously gets a list of competitor ids associated with the current instance
        /// </summary>
        /// <param name="culture">Optional culture in which we want to fetch competitor data (otherwise default is used)</param>
        /// <returns>A list of competitor ids associated with the current instance</returns>
        Task<IEnumerable<URN>> GetCompetitorIdsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{T}"/> representing competitors in the sport event associated with the current instance
        /// </summary>
        /// <param name="culture">The culture in which we want to return competitor data</param>
        /// <returns>A <see cref="Task{T}"/> representing the retrieval operation</returns>
        Task<IEnumerable<ICompetitor>> GetCompetitorsAsync(CultureInfo culture);
    }
}
