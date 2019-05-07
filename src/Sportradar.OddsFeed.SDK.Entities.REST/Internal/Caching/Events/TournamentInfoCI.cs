/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class TournamentInfoCI
    /// </summary>
    /// <seealso cref="SportEventCI" />
    /// <seealso cref="ITournamentInfoCI" />
    public class TournamentInfoCI : SportEventCI, ITournamentInfoCI
    {
        /// <summary>
        /// The category identifier
        /// </summary>
        private URN _categoryId;
        /// <summary>
        /// The tournament coverage
        /// </summary>
        private TournamentCoverageCI _tournamentCoverage;
        /// <summary>
        /// The competitors
        /// </summary>
        private IEnumerable<CompetitorCI> _competitors;
        /// <summary>
        /// The current season information
        /// </summary>
        private CurrentSeasonInfoCI _currentSeasonInfo;
        /// <summary>
        /// The groups
        /// </summary>
        private IEnumerable<GroupCI> _groups;
        /// <summary>
        /// The schedule urns
        /// </summary>
        private IEnumerable<URN> _scheduleUrns;
        /// <summary>
        /// The round
        /// </summary>
        private RoundCI _round;
        /// <summary>
        /// The year
        /// </summary>
        private string _year;
        /// <summary>
        /// The tournament information basic
        /// </summary>
        private TournamentInfoBasicCI _tournamentInfoBasic;
        /// <summary>
        /// The reference identifier
        /// </summary>
        private ReferenceIdCI _referenceId;
        /// <summary>
        /// The season coverage
        /// </summary>
        private SeasonCoverageCI _seasonCoverage;
        /// <summary>
        /// The seasons
        /// </summary>
        private IEnumerable<URN> _seasons;

        /// <summary>
        /// The loaded seasons for tournament
        /// </summary>
        private readonly List<CultureInfo> _loadedSeasons = new List<CultureInfo>();

        /// <summary>
        /// The loaded schedules for tournament
        /// </summary>
        private readonly List<CultureInfo> _loadedSchedules = new List<CultureInfo>();

        /// <summary>
        /// The competitors references
        /// </summary>
        private IDictionary<URN, ReferenceIdCI> _competitorsReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(URN id,
                                IDataRouterManager dataRouterManager,
                                ISemaphorePool semaphorePool,
                                CultureInfo defaultCulture,
                                ObjectCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="eventSummary">The sport event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="currentCulture">A <see cref="CultureInfo" /> of the <see cref="SportEventSummaryDTO" /> instance</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(TournamentInfoDTO eventSummary,
                                IDataRouterManager dataRouterManager,
                                ISemaphorePool semaphorePool,
                                CultureInfo currentCulture,
                                CultureInfo defaultCulture,
                                ObjectCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="fixture">The fixture data</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="currentCulture">A <see cref="CultureInfo" /> of the <see cref="SportEventSummaryDTO" /> instance</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(FixtureDTO fixture,
                                IDataRouterManager dataRouterManager,
                                ISemaphorePool semaphorePool,
                                CultureInfo currentCulture,
                                CultureInfo defaultCulture,
                                ObjectCache fixtureTimestampCache)
            : base(fixture, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(fixture, currentCulture, true);
        }

        /// <summary>
        /// Get category identifier as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{URN}" /> representing the asynchronous operation</returns>
        public async Task<URN> GetCategoryIdAsync()
        {
            if (_categoryId != null)
            {
                return _categoryId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _categoryId;
        }

        /// <summary>
        /// Get tournament coverage as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<TournamentCoverageCI> GetTournamentCoverageAsync()
        {
            if (_tournamentCoverage != null)
            {
                return _tournamentCoverage;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _tournamentCoverage;
        }

        /// <summary>
        /// Get competitors as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<CompetitorCI>> GetCompetitorsAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as List<CultureInfo> ?? cultures.ToList();
            wantedCultures = LanguageHelper.GetMissingCultures(wantedCultures, _competitors?.FirstOrDefault()?.Names.Keys).ToList();
            if (_competitors != null && !wantedCultures.Any())
            {
                return _competitors;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _competitors;
        }

        /// <summary>
        /// Get current season information as an asynchronous operation
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<CurrentSeasonInfoCI> GetCurrentSeasonInfoAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_currentSeasonInfo != null && _currentSeasonInfo.HasTranslationsFor(wantedCultures))
            {
                return _currentSeasonInfo;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _currentSeasonInfo;
        }

        /// <summary>
        /// Get groups as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<GroupCI>> GetGroupsAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_groups != null && !LanguageHelper.GetMissingCultures(wantedCultures, _groups.FirstOrDefault()?.Competitors?.FirstOrDefault()?.Names.Keys).Any())
            {
                return _groups;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _groups;
        }

        /// <summary>
        /// Get schedule as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetScheduleAsync(IEnumerable<CultureInfo> cultures)
        {
            var missingCultures = LanguageHelper.GetMissingCultures(cultures, _loadedSchedules).ToList();
            if (_scheduleUrns == null && missingCultures.Any())
            {
                var tasks = missingCultures.Select(s => DataRouterManager.GetSportEventsForTournamentAsync(Id, s, this)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (tasks.All(a => a.IsCompleted))
                {
                    _loadedSchedules.AddRange(missingCultures);
                    if (tasks.First().Result != null)
                    {
                        _scheduleUrns = tasks.First().Result.Select(s => s.Item1);
                    }
                }
            }

            return _scheduleUrns;
        }

        /// <summary>
        /// Get current round as an asynchronous operation
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<RoundCI> GetCurrentRoundAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_round != null && _round.HasTranslationsFor(wantedCultures))
            {
                return _round;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _round;
        }

        /// <summary>
        /// Get year as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<string> GetYearAsync()
        {
            if (!string.IsNullOrEmpty(_year))
            {
                return _year;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _year;
        }

        /// <summary>
        /// Get tournament information as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<TournamentInfoBasicCI> GetTournamentInfoAsync()
        {
            if (_tournamentInfoBasic != null)
            {
                return _tournamentInfoBasic;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _tournamentInfoBasic;
        }

        /// <summary>
        /// Get reference ids as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<ReferenceIdCI> GetReferenceIdsAsync()
        {
            if (_referenceId != null)
            {
                return _referenceId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _referenceId;
        }

        /// <summary>
        /// Get season coverage as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<SeasonCoverageCI> GetSeasonCoverageAsync()
        {
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _seasonCoverage;
        }

        /// <summary>
        /// Get seasons as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetSeasonsAsync(IEnumerable<CultureInfo> cultures)
        {
            var missingCultures = LanguageHelper.GetMissingCultures(cultures, _loadedSeasons).ToList();
            if (_seasons == null && missingCultures.Any())
            {
                var tasks = missingCultures.Select(s => DataRouterManager.GetSeasonsForTournamentAsync(Id, s, this)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (tasks.All(a => a.IsCompleted))
                {
                    _loadedSeasons.AddRange(missingCultures);
                    _seasons = tasks.First().Result;
                }
            }
            return _seasons;
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync()
        {
            if (_competitorsReferences != null)
            {
                return _competitorsReferences;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _competitorsReferences;
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">if set to <c>true</c> [use lock].</param>
        public void Merge(TournamentInfoDTO dto, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMerge(dto, culture);
                }
            }
            else
            {
                ActualMerge(dto, culture);
            }
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        private void ActualMerge(TournamentInfoDTO dto, CultureInfo culture)
        {
            base.Merge(dto, culture, false);

            if (dto.Category != null)
            {
                _categoryId = dto.Category.Id;
            }
            if (dto.TournamentCoverage != null)
            {
                _tournamentCoverage = new TournamentCoverageCI(dto.TournamentCoverage);
            }
            if (dto.Competitors != null)
            {
                if (_competitors == null)
                {
                    _competitors = new List<CompetitorCI>(dto.Competitors.Select(t => new CompetitorCI(t, culture, DataRouterManager)));
                }
                else
                {
                    MergeCompetitors(dto.Competitors, culture);
                }
                FillCompetitorsReferences(dto.Competitors);
            }
            if (dto.CurrentSeason != null)
            {
                if (_currentSeasonInfo == null)
                {
                    _currentSeasonInfo = new CurrentSeasonInfoCI(dto.CurrentSeason, culture, DataRouterManager);
                }
                else
                {
                    _currentSeasonInfo.Merge(dto.CurrentSeason, culture);
                }
            }
            if (dto.Groups != null)
            {
                if (_groups == null)
                {
                    _groups = new List<GroupCI>(dto.Groups.Select(s => new GroupCI(s, culture, DataRouterManager)));
                }
                else
                {
                    MergeGroups(dto.Groups, culture);
                }
                var comps = new List<CompetitorDTO>();
                foreach (var groupDTO in dto.Groups)
                {
                    comps.AddRange(groupDTO.Competitors);
                }
                FillCompetitorsReferences(comps);
            }
            if (dto.Schedule != null)
            {
                _scheduleUrns = new ReadOnlyCollection<URN>(dto.Schedule.Select(s => s.Id).ToList());
            }
            if (dto.CurrentRound != null)
            {
                if (_round == null)
                {
                    _round = new RoundCI(dto.CurrentRound, culture);
                }
                else
                {
                    _round.Merge(dto.CurrentRound, culture);
                }
            }
            if (!string.IsNullOrEmpty(dto.Year))
            {
                _year = dto.Year;
            }
            if (dto.TournamentInfo != null)
            {
                if (_tournamentInfoBasic == null)
                {
                    _tournamentInfoBasic = new TournamentInfoBasicCI(dto.TournamentInfo, culture, DataRouterManager);
                }
                else
                {
                    _tournamentInfoBasic.Merge(dto.TournamentInfo, culture);
                }
            }
            if (dto.SeasonCoverage != null)
            {
                _seasonCoverage = new SeasonCoverageCI(dto.SeasonCoverage);
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
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeFixture(fixture, culture);
                }
            }
            else
            {
                ActualMergeFixture(fixture, culture);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeFixture(FixtureDTO fixture, CultureInfo culture)
        {
            Merge(new TournamentInfoDTO(fixture), culture, false);

            if (fixture.ReferenceIds != null)
            {
                _referenceId = new ReferenceIdCI(fixture.ReferenceIds);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="tournamentSeasonsDTO">The <see cref="TournamentSeasonsDTO"/></param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(TournamentSeasonsDTO tournamentSeasonsDTO, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    Merge(tournamentSeasonsDTO.Tournament, culture, false);

                    if (tournamentSeasonsDTO.Seasons != null && tournamentSeasonsDTO.Seasons.Any())
                    {
                        _seasons = tournamentSeasonsDTO.Seasons.Select(s => s.Id);
                    }
                }
            }
            else
            {
                Merge(tournamentSeasonsDTO.Tournament, culture, false);

                if (tournamentSeasonsDTO.Seasons != null && tournamentSeasonsDTO.Seasons.Any())
                {
                    _seasons = tournamentSeasonsDTO.Seasons.Select(s => s.Id);
                }
            }
        }

        /// <summary>
        /// Merges the groups
        /// </summary>
        /// <param name="competitors">The groups</param>
        /// <param name="culture">The culture</param>
        private void MergeCompetitors(IEnumerable<CompetitorDTO> competitors, CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (competitors == null)
            {
                return;
            }

            var tempCompetitors = _competitors == null
                ? new List<CompetitorCI>()
                : new List<CompetitorCI>(_competitors);

            foreach (var competitor in competitors)
            {
                var tempCompetitor = tempCompetitors.FirstOrDefault(c => c.Id.Equals(competitor.Id));
                if (tempCompetitor == null)
                {
                    tempCompetitors.Add(new CompetitorCI(competitor, culture, DataRouterManager));
                }
                else
                {
                    tempCompetitor.Merge(competitor, culture);
                }
            }
            _competitors = new ReadOnlyCollection<CompetitorCI>(tempCompetitors);
        }

        /// <summary>
        /// Merges the groups
        /// </summary>
        /// <param name="groups">The groups</param>
        /// <param name="culture">The culture</param>
        private void MergeGroups(IEnumerable<GroupDTO> groups, CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (groups == null)
            {
                return;
            }

            var tmpGroups = _groups == null
                ? new List<GroupCI>()
                : new List<GroupCI>(_groups);

            foreach (var group in groups)
            {
                var tempGroup = tmpGroups.FirstOrDefault(c => c.Name.Equals(group.Name));
                if (tempGroup == null)
                {
                    tmpGroups.Add(new GroupCI(group, culture, DataRouterManager));
                }
                else
                {
                    tempGroup.Merge(group, culture);
                }
            }
            _groups = new ReadOnlyCollection<GroupCI>(tmpGroups);
        }

        private void FillCompetitorsReferences(IEnumerable<CompetitorDTO> competitors)
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
    }
}