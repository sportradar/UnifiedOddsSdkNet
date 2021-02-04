/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent information about a lottery
    /// </summary>
    public interface ILotteryV1 : ILottery
    {
        /// <summary>
        /// Asynchronously gets the list of associated <see cref="IDraw"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<IDraw>> GetDrawsAsync();
    }
}
