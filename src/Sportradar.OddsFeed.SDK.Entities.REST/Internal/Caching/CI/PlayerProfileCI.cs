/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A data-transfer-object representing player's profile
    /// </summary>
    /// <seealso cref="SportEntityCI" />
    public class PlayerProfileCI : SportEntityCI
    {
        private readonly IDataRouterManager _dataRouterManager;
        private string _type;
        private DateTime? _dateOfBirth;
        private int? _height;
        private int? _weight;
        private string _abbreviation;
        private string _gender;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing player name in different languages
        /// </summary>
        public readonly IDictionary<CultureInfo, string> Names;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing player nationality in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _nationalities;

        /// <summary>
        /// Gets a value describing the type(e.g. forward, defense, ...) of the player represented by current instance
        /// </summary>
        public string Type
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _type;
            }
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the date of birth of the player associated with the current instance
        /// </summary>
        public DateTime? DateOfBirth
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _dateOfBirth;
            }
        }

        /// <summary>
        /// Gets the height in centimeters of the player represented by the current instance or a null reference if height is not known
        /// </summary>
        public int? Height
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _height;
            }
        }

        /// <summary>
        /// Gets the weight in kilograms of the player represented by the current instance or a null reference if weight is not known
        /// </summary>
        public int? Weight
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _weight;
            }
        }

        /// <summary>
        /// The abbreviation
        /// </summary>
        /// <remarks>Available via <see cref="PlayerCompetitorDTO"/></remarks>
        public string Abbreviation => _abbreviation;

        /// <summary>
        /// Gets the gender
        /// </summary>
        public string Gender => _gender;

        /// <summary>
        /// Gets the <see cref="IEnumerable{CultureInfo}"/> specifying the languages for which the current instance has translations
        /// </summary>
        /// <value>The fetched cultures</value>
        private readonly IEnumerable<CultureInfo> _fetchedCultures;

        private readonly object _lock = new object();
        private readonly CultureInfo _primaryCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileCI"/> class
        /// </summary>
        /// <param name="profile">The <see cref="PlayerProfileDTO"/> used to create instance</param>
        /// <param name="culture">The culture of the <see cref="PlayerProfileDTO"/> used to create new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="PlayerProfileDTO"/></param>
        internal PlayerProfileCI(PlayerProfileDTO profile, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(profile)
        {
            Contract.Requires(profile != null);
            Contract.Requires(culture != null);
            Contract.Requires(dataRouterManager != null);

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _nationalities = new Dictionary<CultureInfo, string>();
            Merge(profile, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileCI"/> class
        /// </summary>
        /// <param name="playerCompetitor">The <see cref="PlayerCompetitorDTO"/> used to create instance</param>
        /// <param name="culture">The culture of the <see cref="PlayerCompetitorDTO"/> used to create new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="PlayerCompetitorDTO"/></param>
        internal PlayerProfileCI(PlayerCompetitorDTO playerCompetitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(playerCompetitor)
        {
            Contract.Requires(playerCompetitor != null);
            Contract.Requires(culture != null);
            Contract.Requires(dataRouterManager != null);

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _nationalities = new Dictionary<CultureInfo, string>();
            Merge(playerCompetitor, culture);
        }

        /// <summary>
        /// Merges the specified <see cref="PlayerProfileDTO"/> into instance
        /// </summary>
        /// <param name="profile">The <see cref="PlayerProfileDTO"/> used to merge into instance</param>
        /// <param name="culture">The culture of the <see cref="PlayerProfileDTO"/> used to merge</param>
        internal void Merge(PlayerProfileDTO profile, CultureInfo culture)
        {
            Contract.Requires(profile != null);
            Contract.Requires(culture != null);

            _type = profile.Type;
            _dateOfBirth = profile.DateOfBirth;
            Names[culture] = profile.Name;
            _nationalities[culture] = profile.Nationality;
            _height = profile.Height;
            _weight = profile.Weight;
            _gender = profile.Gender;

            if (string.IsNullOrEmpty(_abbreviation))
            {
                _abbreviation = SdkInfo.GetAbbreviationFromName(profile.Name);
            }

            ((List<CultureInfo>)_fetchedCultures).Add(culture);
        }

        /// <summary>
        /// Merges the specified <see cref="PlayerCompetitorDTO"/> into instance
        /// </summary>
        /// <param name="playerCompetitor">The <see cref="PlayerCompetitorDTO"/> used to merge into instance</param>
        /// <param name="culture">The culture of the <see cref="PlayerCompetitorDTO"/> used to merge</param>
        internal void Merge(PlayerCompetitorDTO playerCompetitor, CultureInfo culture)
        {
            Contract.Requires(playerCompetitor != null);
            Contract.Requires(culture != null);

            Names[culture] = playerCompetitor.Name;
            _nationalities[culture] = playerCompetitor.Nationality;
            _abbreviation = string.IsNullOrEmpty(playerCompetitor.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(playerCompetitor.Name)
                : playerCompetitor.Abbreviation;
        }

        /// <summary>
        /// Gets the name of the player in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language if it exists. Null otherwise.</returns>
        public string GetName(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (!Names.ContainsKey(culture))
            {
                FetchProfileIfNeeded(culture);
            }

            return Names.ContainsKey(culture)
                ? Names[culture]
                : null;
        }

        /// <summary>
        /// Gets the nationality of the player in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned nationality</param>
        /// <returns>The nationality of the player in the specified language if it exists. Null otherwise.</returns>
        public string GetNationality(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (!_nationalities.ContainsKey(culture))
            {
                FetchProfileIfNeeded(culture);
            }

            return _nationalities.ContainsKey(culture)
                ? _nationalities[culture]
                : null;
        }

        private void FetchProfileIfNeeded(CultureInfo culture)
        {
            if (_fetchedCultures.Contains(culture))
            {
                return;
            }

            lock (_lock)
            {
                if (!_fetchedCultures.Contains(culture))
                {

                    var task = Task.Run(async () =>
                    {
                        await _dataRouterManager.GetPlayerProfileAsync(Id, culture, null).ConfigureAwait(false);
                    });
                    task.Wait();
                }
            }
        }
    }
}