/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport events of match type
    /// </summary>
    public interface IMatchV2 : IMatchV1
    {
        /// <summary>
        /// Asynchronously gets the associated coverage info
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the retrieval operation</returns>
        Task<ICoverageInfo> GetCoverageInfoAsync();
    }
}
