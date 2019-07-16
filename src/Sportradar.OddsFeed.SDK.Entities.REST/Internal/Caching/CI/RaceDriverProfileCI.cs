/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about race driver profile
    /// </summary>
    public class RaceDriverProfileCI
    {
        public URN RaceDriverId { get; }

        public URN RaceTeamId { get; }

        public CarCI Car { get; }

        public RaceDriverProfileCI(RaceDriverProfileDTO item)
        {
            Contract.Requires(item != null);

            RaceDriverId = item.RaceDriverId;
            RaceTeamId = item.RaceTeamId;
            Car = item.Car != null ? new CarCI(item.Car) : null;
        }
    }
}
