/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for event player assist
    /// </summary>
    /// <seealso cref="CacheItem" />
    public class EventPlayerAssistCI : CacheItem
    {
        public string Type { get; }

        public EventPlayerAssistCI(EventPlayerAssistDTO dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Contract.Requires(dto != null);

            Type = dto.Type;
        }
    }
}
