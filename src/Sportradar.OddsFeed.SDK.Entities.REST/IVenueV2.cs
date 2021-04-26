/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a sport event venue
    /// </summary>
    public interface IVenueV2 : IVenueV1
    {
        /// <summary>
        /// Gets the course
        /// </summary>
        /// <value>The course</value>
        IEnumerable<IHole> Course { get; }
    }
}
