/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about weather
    /// </summary>
    public class WeatherInfoCI
    {
        /// <summary>
        /// Gets the temperature in degrees celsius or a null reference if the temperature is not known
        /// </summary>
        internal int? TemperatureCelsius { get; }

        internal string Wind { get; }

        internal string WindAdvantage { get; }

        internal string Pitch { get; }

        internal string WeatherConditions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfoCI"/> class.
        /// </summary>
        /// <param name="dto">The <see cref="WeatherInfoDTO"/> used to create new instance</param>
        internal WeatherInfoCI(WeatherInfoDTO dto)
        {
            Contract.Requires(dto != null);

            TemperatureCelsius = dto.TemperatureCelsius;
            Wind = dto.Wind;
            WindAdvantage = dto.WindAdvantage;
            Pitch = dto.Pitch;
            WeatherConditions = dto.WeatherConditions;
        }
    }
}
