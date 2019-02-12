/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class CompetitionCI
    /// </summary>
    /// <seealso cref="ISportEventCI" />
    /// <seealso cref="ICompetitionCI" />
    public class CompetitionCI : SportEventCI, ICompetitionCI
    {
        /// <summary>
        /// The sport event status
        /// </summary>
        private SportEventStatusCI _sportEventStatus;
        /// <summary>
        /// The booking status
        /// </summary>
        private BookingStatus? _bookingStatus;
        /// <summary>
        /// The venue
        /// </summary>
        private VenueCI _venue;
        /// <summary>
        /// The conditions
        /// </summary>
        private SportEventConditionsCI _conditions;
        /// <summary>
        /// The competitors
        /// </summary>
        protected IEnumerable<TeamCompetitorCI> Competitors;
        /// <summary>
        /// The reference identifier
        /// </summary>
        private ReferenceIdCI _referenceId;
        /// <summary>
        /// The competitors qualifiers
        /// </summary>
        private IDictionary<URN, string> _competitorsQualifiers;
        /// <summary>
        /// The competitors references
        /// </summary>
        private IDictionary<URN, ReferenceIdCI> _competitorsReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ...)</param>
        public CompetitionCI(URN id,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo defaultCulture)
            : base(id, dataRouterManager, semaphorePool, defaultCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        public CompetitionCI(CompetitionDTO eventSummary,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo currentCulture,
                             CultureInfo defaultCulture)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture)
        {
            Contract.Requires(eventSummary != null);
            Contract.Requires(currentCulture != null);

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        public CompetitionCI(TournamentInfoDTO eventSummary,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo currentCulture,
                            CultureInfo defaultCulture)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture)
        {
            Contract.Requires(eventSummary != null);
            Contract.Requires(currentCulture != null);

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Get sport event status as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<SportEventStatusCI> GetSportEventStatusAsync()
        {
            await FetchMissingSummary(new[] { DefaultCulture }).ConfigureAwait(false);
            return _sportEventStatus;
        }

        /// <summary>
        /// Get booking status as an asynchronous operation
        /// </summary>
        /// <returns>Task&lt;System.Nullable&lt;BookingStatus&gt;&gt;</returns>
        public async Task<BookingStatus?> GetBookingStatusAsync()
        {
            if (LoadedFixtures.Any() || Id.TypeGroup == ResourceTypeGroup.STAGE || _bookingStatus != null)
            {
                return _bookingStatus;
            }
            await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            return _bookingStatus;
        }

        /// <summary>
        /// get venue as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<VenueCI> GetVenueAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_venue != null && _venue.HasTranslationsFor(cultureInfos))
            {
                return _venue;
            }
            await FetchMissingSummary(cultureInfos).ConfigureAwait(false);
            return _venue;
        }

        /// <summary>
        /// get conditions as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<SportEventConditionsCI> GetConditionsAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_conditions?.Referee != null && _conditions.Referee.HasTranslationsFor(cultureInfos))
            {
                return _conditions;
            }
            await FetchMissingSummary(cultureInfos).ConfigureAwait(false);
            return _conditions;
        }

        /// <summary>
        /// get competitors as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<IEnumerable<TeamCompetitorCI>> GetCompetitorsAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures.ToList();
            if (Competitors != null
                && Competitors.Any()
                && !LanguageHelper.GetMissingCultures(wantedCultures, Competitors.First().Names.Keys.ToList()).ToList().Any())
            {
                return Competitors;
            }
            await FetchMissingSummary(wantedCultures).ConfigureAwait(false);
            return Competitors;
        }
        /// <summary>
        /// get reference ids as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<ReferenceIdCI> GetReferenceIdsAsync()
        {
            if (_referenceId != null)
            {
                return _referenceId;
            }
            await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            return _referenceId;
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync()
        {
            if (!LoadedFixtures.Any())
            {
                await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            }
            return _competitorsReferences;
        }

        /// <summary>
        /// Asynchronously get the list of available team qualifiers
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<URN, string>> GetCompetitorsQualifiersAsync()
        {
            if (!LoadedSummaries.Any())
            {
                await FetchMissingSummary(new[] { DefaultCulture }).ConfigureAwait(false);
            }
            return _competitorsQualifiers;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(CompetitionDTO eventSummary, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMerge(eventSummary, culture);
                }
            }
            else
            {
                ActualMerge(eventSummary, culture);
            }
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        private void ActualMerge(CompetitionDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Status != null)
            {
                _sportEventStatus = new SportEventStatusCI(eventSummary.Status);
            }

            if (eventSummary.Venue != null)
            {
                if (_venue == null)
                {
                    _venue = new VenueCI(eventSummary.Venue, culture);
                }
                else
                {
                    _venue.Merge(eventSummary.Venue, culture);
                }
            }
            if (eventSummary.Conditions != null)
            {
                if (_conditions == null)
                {
                    _conditions = new SportEventConditionsCI(eventSummary.Conditions, culture);
                }
                else
                {
                    _conditions.Merge(eventSummary.Conditions, culture);
                }
            }
            if (eventSummary.Competitors != null)
            {
                if (Competitors == null)
                {
                    Competitors = new List<TeamCompetitorCI>(eventSummary.Competitors.Select(t => new TeamCompetitorCI(t, culture, DataRouterManager)));
                }
                else
                {
                    MergeCompetitors(eventSummary.Competitors, culture);
                }
                GenerateMatchName(eventSummary.Competitors, culture);
                FillCompetitorsQualifiers(eventSummary.Competitors);
                FillCompetitorsReferences(eventSummary.Competitors);
            }
            if (eventSummary.BookingStatus != null)
            {
                _bookingStatus = eventSummary.BookingStatus;
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void MergeFixture(FixtureDTO fixture, CultureInfo culture, bool useLock)
        {
            //Merge(fixture, culture);

            if (useLock)
            {
                lock (MergeLock)
                {
                    if (fixture.ReferenceIds != null)
                    {
                        _referenceId = new ReferenceIdCI(fixture.ReferenceIds);
                    }
                    if (fixture.BookingStatus != null)
                    {
                        _bookingStatus = fixture.BookingStatus;
                    }
                }
            }
            else
            {
                if (fixture.ReferenceIds != null)
                {
                    _referenceId = new ReferenceIdCI(fixture.ReferenceIds);
                }
                if (fixture.BookingStatus != null)
                {
                    _bookingStatus = fixture.BookingStatus;
                }
            }
        }

        /// <summary>
        /// Merges the competitors
        /// </summary>
        /// <param name="competitors">The competitors</param>
        /// <param name="culture">The culture</param>
        private void MergeCompetitors(IEnumerable<TeamCompetitorDTO> competitors, CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (competitors == null)
            {
                return;
            }

            if (Competitors == null)
            {
                Competitors = new ReadOnlyCollection<TeamCompetitorCI>(competitors.Select(s=>new TeamCompetitorCI(s, culture, DataRouterManager)).ToList());
                return;
            }

            var tempCompetitors = new List<TeamCompetitorCI>();

            foreach (var competitor in competitors)
            {
                var tempCompetitor = Competitors.FirstOrDefault(c => c.Id.Equals(competitor.Id));
                if (tempCompetitor == null)
                {
                    tempCompetitor = new TeamCompetitorCI(competitor, culture, DataRouterManager);
                }
                else
                {
                    tempCompetitor.Merge(competitor, culture);
                }
                tempCompetitors.Add(tempCompetitor);
            }
            Competitors = new ReadOnlyCollection<TeamCompetitorCI>(tempCompetitors);
        }

        private void GenerateMatchName(IEnumerable<TeamCompetitorDTO> competitors, CultureInfo culture)
        {
            var teamCompetitorDtos = competitors.ToList();
            if (Id.TypeGroup == ResourceTypeGroup.MATCH && teamCompetitorDtos.Count == 2)
            {
                string name;
                if (Names.TryGetValue(culture, out name) && !string.IsNullOrEmpty(name))
                {
                    return;
                }

                var homeTeam = teamCompetitorDtos.FirstOrDefault(f => f.Qualifier == "home");
                if (homeTeam == null)
                {
                    return;
                }
                var awayTeam = teamCompetitorDtos.FirstOrDefault(f => f.Qualifier == "away");
                if (awayTeam == null)
                {
                    return;
                }
                Names[culture] = homeTeam.Name + " vs. " + awayTeam.Name;
            }
        }

        private void FillCompetitorsQualifiers(IEnumerable<TeamCompetitorDTO> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsQualifiers == null)
            {
                _competitorsQualifiers = new Dictionary<URN, string>();
            }
            foreach (var competitor in competitors)
            {
                if (!string.IsNullOrEmpty(competitor.Qualifier))
                {
                    _competitorsQualifiers[competitor.Id] = competitor.Qualifier;
                }
            }
        }

        private void FillCompetitorsReferences(IEnumerable<TeamCompetitorDTO> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsReferences == null)
            {
                _competitorsReferences = new Dictionary<URN, ReferenceIdCI>();
            }
            foreach (var competitor in competitors)
            {
                if (competitor.ReferenceIds != null && competitor.ReferenceIds.Any())
                {
                    if (_competitorsReferences.ContainsKey(competitor.Id))
                    {
                        var compRefs = _competitorsReferences[competitor.Id];
                        compRefs.Merge(competitor.ReferenceIds, true);
                    }
                    else
                    {
                        _competitorsReferences[competitor.Id] = new ReferenceIdCI(competitor.ReferenceIds);
                    }
                }
            }
        }

        /// <summary>
        /// Change booking status to Booked
        /// </summary>
        public void Book()
        {
            _bookingStatus = BookingStatus.Booked;
        }
    }
}
