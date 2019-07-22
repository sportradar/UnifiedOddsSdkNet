/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports
{
    /// <summary>
    /// Represents a cached tournament entity
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class CategoryCI : CacheItem
    {
        /// <summary>
        /// Gets the <see cref="URN"/> specifying the id of the parent sport
        /// </summary>
        public URN SportId { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{URN}"/> containing the ids of child tournaments
        /// </summary>
        public IEnumerable<URN> TournamentIds { get; }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCI"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CategoryDTO"/> containing the category data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        /// <param name="sportId">The id of the parent sport</param>
        public CategoryCI(CategoryDTO data, CultureInfo culture, URN sportId)
            : base(data.Id, data.Name, culture)
        {
            Contract.Requires(sportId != null);

            TournamentIds = data.Tournaments == null ? null : new ReadOnlyCollection<URN>(data.Tournaments.Select(i => i.Id).ToList());
            SportId = sportId;
            CountryCode = data.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCI"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CategoryDTO"/> containing the category data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        /// <param name="sportId">The id of the parent sport</param>
        /// <param name="tournamentIds">The list of tournament ids</param>
        public CategoryCI(CategorySummaryDTO data, CultureInfo culture, URN sportId, IEnumerable<URN> tournamentIds)
            : base(data.Id, data.Name, culture)
        {
            Contract.Requires(sportId != null);

            TournamentIds = tournamentIds == null ? null : new ReadOnlyCollection<URN>(tournamentIds.ToList());
            SportId = sportId;
            CountryCode = data.CountryCode;
        }
    }
}
