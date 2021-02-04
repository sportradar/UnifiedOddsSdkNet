/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProviderV8 : ISportDataProviderV7
    {
        /// <summary>
        /// Gets a <see cref="ISportEvent"/> derived class representing the specified sport event in language specified by <code>culture</code> or a null reference if the sport event with
        /// specified <code>id</code> does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the sport event to retrieve</param>
        /// <param name="sportId">A <see cref="URN"/> of the sport this event belongs to</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ISportEvent"/> derived class representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        ISportEvent GetSportEvent(URN id, URN sportId = null, CultureInfo culture = null);
    }
}
