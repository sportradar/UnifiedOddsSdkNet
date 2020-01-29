/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a sport event venue
    /// </summary>
    public interface IVenueV1 : IVenue
    {
        /// <summary>
        /// Gets a state of the venue represented by current <see cref="IVenue" /> instance
        /// </summary>
        string State { get; }
    }
}
