/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about race driver profile
    /// </summary>
    public class CarCI
    {
        public string Name { get; }

        public string Chassis { get; }

        public string EngineName { get; }

        public CarCI(CarDTO item)
        {
            Contract.Requires(item != null);

            Name = item.Name;
            Chassis = item.Chassis;
            EngineName = item.EngineName;
        }
    }
}