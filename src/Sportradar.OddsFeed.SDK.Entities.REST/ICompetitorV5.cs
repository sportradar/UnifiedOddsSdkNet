/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitorV5 : ICompetitorV4
    {
        /// <summary>
        /// Gets associated sport
        /// </summary>
        /// <returns>The associated sport</returns>
        Task<ISport> GetSportAsync();

        /// <summary>
        /// Gets associated category
        /// </summary>
        /// <returns>The associated category</returns>
        Task<ICategorySummary> GetCategoryAsync();
    }
}
