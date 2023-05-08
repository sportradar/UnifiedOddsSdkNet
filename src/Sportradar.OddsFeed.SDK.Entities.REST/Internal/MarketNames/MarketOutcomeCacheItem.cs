﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal class MarketOutcomeCacheItem
    {
        internal string Id { get; }

        private readonly IDictionary<CultureInfo, string> _names;

        private readonly IDictionary<CultureInfo, string> _descriptions;

        internal MarketOutcomeCacheItem(OutcomeDescriptionDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Id = dto.Id;
            _names = new Dictionary<CultureInfo, string> { {culture, dto.Name} };
            _descriptions = string.IsNullOrEmpty(dto.Description)
                ? new Dictionary<CultureInfo, string>()
                : new Dictionary<CultureInfo, string> {{culture, dto.Description}};
        }

        internal string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            string name;
            if (_names.TryGetValue(culture, out name))
            {
                return name;
            }
            return null;
        }

        internal string GetDescription(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            string description;
            if (_descriptions.TryGetValue(culture, out description))
            {
                return description;
            }
            return null;
        }

        internal void Merge(OutcomeDescriptionDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _names[culture] = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description))
            {
                _descriptions[culture] = dto.Description;
            }
        }
    }
}
