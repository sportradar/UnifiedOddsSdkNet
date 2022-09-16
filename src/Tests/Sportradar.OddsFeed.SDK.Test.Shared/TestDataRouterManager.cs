﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.REST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestDataRouterManager : IDataRouterManager
    {
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        private static readonly string DirPath = Directory.GetCurrentDirectory() + @"\REST XMLs\";

        private const string FixtureXml = "fixtures.{culture}.xml";
        private const string SportCategoriesXml = "sport_categories.{culture}.xml";
        private const string ScheduleXml = "schedule.{culture}.xml";
        private const string TourScheduleXml = "tournament_schedule.{culture}.xml";
        private const string SportsXml = "sports.{culture}.xml";
        private const string MatchDetailsXml = "event_details.{culture}.xml";
        private const string RaceDetailsXml = "race_summary.xml";
        private const string TournamentScheduleXml = "tournaments.{culture}.xml";
        private const string TournamentInfoXml = "tournament_info.xml";
        private const string TournamentExtraInfoXml = "tournament_info_extra.xml";
        private const string PlayerProfileXml = "{culture}.player.1.xml";
        private const string CompetitorProfileXml = "{culture}.competitor.1.xml";
        private const string SimpleTeamProfileXml = "{culture}.simpleteam.1.xml";

        private readonly ICacheManager _cacheManager;
        private readonly IDeserializer<RestMessage> _restDeserializer = new Deserializer<RestMessage>();
        private TimeSpan _delay = TimeSpan.Zero;
        private bool _delayVariable;
        private int _delayPercent = 10;

        public int TotalRestCalls => RestCalls.Sum(s => s.Value);

        public readonly Dictionary<string, int> RestCalls;

        private readonly object _lock = new object();

        internal TestDataRouterManager(ICacheManager cacheManager)
        {
            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();

            _cacheManager = cacheManager;
            RestCalls = new Dictionary<string, int>();
        }

        private string GetFile(string template, CultureInfo culture)
        {
            var filePath = FileHelper.FindFile(template.Replace("{culture}", culture.TwoLetterISOLanguageName));
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = FileHelper.FindFile(template.Replace("{culture}", TestData.Culture.TwoLetterISOLanguageName));
            }
            var fi = new FileInfo(filePath);
            if (fi.Exists)
            {
                return fi.FullName;

            }
            return string.Empty; // ""
            //throw new ArgumentException($"Missing file {filePath}, for template {template} and lang:{culture.TwoLetterISOLanguageName}.");
        }

        private void RecordCall(string callType)
        {
            lock (_lock)
            {
                if (RestCalls.ContainsKey(callType))
                {
                    RestCalls.TryGetValue(callType, out var value);
                    RestCalls[callType] = value + 1;
                }
                else
                {
                    RestCalls.Add(callType, 1);
                }
            }
        }

        /// <summary>
        /// Gets the count of the calls (per specific method or all together if not type provided)
        /// </summary>
        /// <param name="callType">Type of the call</param>
        /// <returns>The count calls</returns>
        public int GetCallCount(string callType)
        {
            if (string.IsNullOrEmpty(callType))
            {
                return TotalRestCalls;
            }

            if (!RestCalls.ContainsKey(callType))
            {
                return 0;
            }

            RestCalls.TryGetValue(callType, out var value);
            return value;
        }

        public void AddDelay(TimeSpan delay, bool variable = false, int percentOfRequests = 20)
        {
            _delay = delay;
            _delayVariable = variable;
            _delayPercent = percentOfRequests;
        }

        private async Task ExecuteDelayAsync(URN id, CultureInfo culture)
        {
            await ExecuteDelayAsync(id.ToString(), culture).ConfigureAwait(false);
        }

        private async Task ExecuteDelayAsync(string id, CultureInfo culture)
        {
            if (_delay != TimeSpan.Zero)
            {
                if (_delayPercent < 1)
                {
                    return;
                }
                if (_delayPercent < 100)
                {
                    var percent = StaticRandom.I100;
                    if (percent > _delayPercent)
                    {
                        return;
                    }
                }
                var delayMs = (int)_delay.TotalMilliseconds;
                if (_delayVariable)
                {
                    delayMs = StaticRandom.I(delayMs);
                }
                Debug.WriteLine($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs} ms START");
                //Log.LogInformation($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs}ms");
                //Thread.Sleep(delayMs);
                await Task.Delay(delayMs).ConfigureAwait(false);
                Debug.WriteLine($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs} ms END");
            }
        }

        public async Task GetSportEventSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetSportEventSummaryAsync");
            var strId = id?.ToString().Replace(":", "_") ?? string.Empty;
            var filePath = GetFile($"summary_{strId}.{culture.TwoLetterISOLanguageName}.xml", culture);
            if (string.IsNullOrEmpty(filePath) && id?.TypeGroup != ResourceTypeGroup.BASIC_TOURNAMENT && id?.TypeGroup != ResourceTypeGroup.TOURNAMENT && id?.TypeGroup != ResourceTypeGroup.SEASON)
            {
                filePath = GetFile(MatchDetailsXml, culture);
            }

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.WriteLine($"GetSportEventSummaryAsync for {id} and {culture.TwoLetterISOLanguageName}: no result.");
                return;
            }
            var mapper = new SportEventSummaryMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(_restDeserializer.Deserialize(stream)).Map();
            if (result != null)
            {
                await LogSaveDtoAsync(id, result, culture, DtoType.SportEventSummary, requester).ConfigureAwait(false);
            }
        }

        public async Task GetSportEventFixtureAsync(URN id, CultureInfo culture, bool useCachedProvider, ISportEventCI requester)
        {
            RecordCall("GetSportEventFixtureAsync");
            var filePath = GetFile(FixtureXml, culture);
            var restDeserializer = new Deserializer<fixturesEndpoint>();
            var mapper = new FixtureMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result == null)
            {
                Debug.WriteLine($"GetSportEventFixtureAsync for {id} and {culture.TwoLetterISOLanguageName}: no result.");
                return;
            }

            await LogSaveDtoAsync(id, result, culture, DtoType.Fixture, requester).ConfigureAwait(false);
        }

        public async Task GetAllTournamentsForAllSportAsync(CultureInfo culture)
        {
            RecordCall("GetAllTournamentsForAllSportAsync");
            var filePath = GetFile(TournamentScheduleXml, culture);
            var restDeserializer = new Deserializer<tournamentsEndpoint>();
            var mapper = new TournamentsMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("alltournamentsallsports", culture).ConfigureAwait(false);

            if (result == null)
            {
                Debug.WriteLine($"GetAllTournamentsForAllSportAsync for {culture.TwoLetterISOLanguageName}: no result.");
                return;
            }

            await LogSaveDtoAsync(URN.Parse($"sr:sports:{result.Items.Count()}"), result, culture, DtoType.SportList, null).ConfigureAwait(false);
        }

        public async Task GetSportCategoriesAsync(URN id, CultureInfo culture)
        {
            RecordCall("GetSportCategoriesAsync");
            var filePath = GetFile(SportCategoriesXml, culture);
            var restDeserializer = new Deserializer<sportCategoriesEndpoint>();
            var mapper = new SportCategoriesMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result?.Categories != null)
            {
                foreach (var category in result.Categories)
                {
                    await LogSaveDtoAsync(id, category, culture, DtoType.Category, null).ConfigureAwait(false);
                }
            }
        }

        public async Task GetAllSportsAsync(CultureInfo culture)
        {
            RecordCall("GetAllSportsAsync");
            var filePath = GetFile(SportsXml, culture);
            var restDeserializer = new Deserializer<sportsEndpoint>();
            var mapper = new SportsMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("allsports", culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:sports:{result.Items.Count()}"), result, culture, DtoType.SportList, null).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<Tuple<URN, URN>>> GetLiveSportEventsAsync(CultureInfo culture)
        {
            RecordCall("GetLiveSportEventsAsync");
            var filePath = GetFile("live_events.xml", culture);
            var restDeserializer = new Deserializer<scheduleEndpoint>();
            var mapper = new DateScheduleMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("getlivesports", culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result.Items.Count()}"), result, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                var urns = new List<Tuple<URN, URN>>();
                foreach (var item in result.Items)
                {
                    urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForDateAsync(DateTime date, CultureInfo culture)
        {
            RecordCall("GetSportEventsForDateAsync");
            var filePath = GetFile(ScheduleXml, culture);
            var restDeserializer = new Deserializer<scheduleEndpoint>();
            var mapper = new DateScheduleMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(date.ToString("yyyy-M-d"), culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result.Items.Count()}"), result, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                var urns = new List<Tuple<URN, URN>>();
                foreach (var item in result.Items)
                {
                    urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetSportEventsForTournamentAsync");
            var filePath = GetFile(TourScheduleXml, culture);
            var restDeserializer = new Deserializer<tournamentSchedule>();
            var mapper = new TournamentScheduleMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result.Items.Count()}"), result, culture, DtoType.SportEventSummaryList, requester).ConfigureAwait(false);
                var urns = new List<Tuple<URN, URN>>();
                foreach (var item in result.Items)
                {
                    urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task GetPlayerProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetPlayerProfileAsync");
            var filePath = GetFile($"{culture.TwoLetterISOLanguageName}.player.{id?.Id ?? 1}.xml", culture);
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetFile(PlayerProfileXml, culture);
            }
            var restDeserializer = new Deserializer<playerProfileEndpoint>();
            var mapper = new PlayerProfileMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(id, result, culture, DtoType.PlayerProfile, requester).ConfigureAwait(false);
            }
        }

        public Task GetCompetitorAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetCompetitorAsync");
            return id.IsSimpleTeam()
                ? GetSimpleTeamProfileAsync(id, culture, requester)
                : GetCompetitorProfileAsync(id, culture, requester);
        }

        private async Task GetCompetitorProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            Debug.Print($"DRM-GetCompetitorProfileAsync for {id} and culture {culture.TwoLetterISOLanguageName} - START");
            var filePath = GetFile($"{culture.TwoLetterISOLanguageName}.competitor.{id?.Id ?? 1}.xml", culture);
            CompetitorProfileDTO dto;

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                //filePath = GetFile(CompetitorProfileXml, culture);
                dto = new CompetitorProfileDTO(MessageFactoryRest.GetCompetitorProfileEndpoint(id == null ? 1 : (int)id.Id, StaticRandom.I(15)));
            }
            else
            {
                var restDeserializer = new Deserializer<competitorProfileEndpoint>();
                var mapper = new CompetitorProfileMapperFactory();
                var stream = FileHelper.OpenFile(filePath);
                dto = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            }

            if (dto != null)
            {
                await LogSaveDtoAsync(id, dto, culture, DtoType.CompetitorProfile, requester).ConfigureAwait(false);
            }
            Debug.Print($"DRM-GetCompetitorProfileAsync for {id} and culture {culture.TwoLetterISOLanguageName} - END");
        }

        private async Task GetSimpleTeamProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            var filePath = GetFile($"{culture.TwoLetterISOLanguageName}.simpleteam.{id?.Id ?? 1}.xml", culture);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                filePath = GetFile(SimpleTeamProfileXml, culture);
            }
            var restDeserializer = new Deserializer<simpleTeamProfileEndpoint>();
            var mapper = new SimpleTeamProfileMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(id, result, culture, DtoType.SimpleTeamProfile, requester).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<URN>> GetSeasonsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetSeasonsForTournamentAsync");
            var filePath = GetFile("tournament_seasons.{culture}.xml", culture);
            var restDeserializer = new Deserializer<tournamentSeasons>();
            var mapper = new TournamentSeasonsMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(id, result, culture, DtoType.TournamentSeasons, requester).ConfigureAwait(false);
                var urns = new List<URN>();
                foreach (var item in result.Seasons)
                {
                    urns.Add(item.Id);
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task<MatchTimelineDTO> GetInformationAboutOngoingEventAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetInformationAboutOngoingEventAsync");
            var filePath = GetFile("match_timeline.{culture}.xml", culture);
            var restDeserializer = new Deserializer<matchTimelineEndpoint>();
            var mapper = new MatchTimelineMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(id, result, culture, DtoType.MatchTimeline, requester).ConfigureAwait(false);
            }

            return result;
        }

        public async Task GetMarketDescriptionsAsync(CultureInfo culture)
        {
            RecordCall("GetMarketDescriptionsAsync");
            var filePath = GetFile("invariant_market_descriptions.{culture}.xml", culture);
            var restDeserializer = new Deserializer<market_descriptions>();
            var mapper = new MarketDescriptionsMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("market_description", culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse("sr:markets:" + result.Items?.Count()), result, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
            }
        }

        public async Task GetVariantMarketDescriptionAsync(int id, string variant, CultureInfo culture)
        {
            RecordCall("GetVariantMarketDescriptionAsync");
            var filePath = GetFile("variant_market_description.{culture}.xml", culture);
            var restDeserializer = new Deserializer<market_descriptions>();
            var mapper = new MarketDescriptionMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("sr:variant:" + result.Id, culture).ConfigureAwait(false);

            await LogSaveDtoAsync(URN.Parse("sr:variant:" + result.Id), result, culture, DtoType.MarketDescription, null).ConfigureAwait(false);
        }

        public async Task GetVariantDescriptionsAsync(CultureInfo culture)
        {
            RecordCall("GetVariantDescriptionsAsync");
            var filePath = GetFile("variant_market_descriptions.{culture}.xml", culture);
            var restDeserializer = new Deserializer<variant_descriptions>();
            var mapper = new VariantDescriptionsMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("variant_description", culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse("sr:variants:" + result.Items?.Count()), result, culture, DtoType.VariantDescriptionList, null).ConfigureAwait(false);
            }
        }

        public async Task GetDrawSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetDrawSummaryAsync");
            var filePath = GetFile("draw_summary.{culture}.xml", culture);
            var restDeserializer = new Deserializer<draw_summary>();
            var mapper = new DrawSummaryMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(result.Id, result, culture, DtoType.LotteryDraw, requester).ConfigureAwait(false);
            }
        }

        public async Task GetDrawFixtureAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetDrawFixtureAsync");
            var filePath = GetFile("draw_fixture.{culture}.xml", culture);
            var restDeserializer = new Deserializer<draw_fixtures>();
            var mapper = new DrawFixtureMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(result.Id, result, culture, DtoType.LotteryDraw, requester).ConfigureAwait(false);
            }
        }

        public async Task GetLotteryScheduleAsync(URN lotteryId, CultureInfo culture, ISportEventCI requester)
        {
            RecordCall("GetLotteryScheduleAsync");
            var filePath = GetFile("lottery_schedule.{culture}.xml", culture);
            var restDeserializer = new Deserializer<lottery_schedule>();
            var mapper = new LotteryScheduleMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync(lotteryId, culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(result.Id, result, culture, DtoType.Lottery, requester).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="ignoreFail">if the fail should be ignored - when user does not have access</param>
        /// <returns>The list of combination of id of the lottery and associated sport id</returns>
        /// <remarks>This gets called only if WNS is available</remarks>
        public async Task<IEnumerable<Tuple<URN, URN>>> GetAllLotteriesAsync(CultureInfo culture, bool ignoreFail)
        {
            RecordCall("GetAllLotteriesAsync");
            var filePath = GetFile("lotteries.{culture}.xml", culture);
            var restDeserializer = new Deserializer<lotteries>();
            var mapper = new LotteriesMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("all_lotteries", culture).ConfigureAwait(false);

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:lotteries:{result.Items.Count()}"), result, culture, DtoType.LotteryList, null).ConfigureAwait(false);
                var urns = new List<Tuple<URN, URN>>();
                foreach (var item in result.Items)
                {
                    urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task<IAvailableSelections> GetAvailableSelectionsAsync(URN id)
        {
            RecordCall("GetAvailableSelectionsAsync");
            var filePath = GetFile("available_selections.xml", CultureInfo.CurrentCulture);
            var restDeserializer = new Deserializer<AvailableSelectionsType>();
            var mapper = new AvailableSelectionsMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("available_selections", CultureInfo.CurrentCulture).ConfigureAwait(false);

            if (id.Id == 0)
            {
                result = null;
            }
            else if (id.Id != 31561675)
            {
                result = new AvailableSelectionsDto(MessageFactoryRest.GetAvailableSelections(id, StaticRandom.I100));
            }

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:sels:{result.Markets.Count}"), result, CultureInfo.CurrentCulture, DtoType.AvailableSelections, null).ConfigureAwait(false);
                return new AvailableSelections(result);
            }

            return null;
        }

        public async Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            RecordCall("CalculateProbabilityAsync");
            var filePath = GetFile("calculate_response.xml", CultureInfo.CurrentCulture);
            var restDeserializer = new Deserializer<CalculationResponseType>();
            var mapper = new CalculationMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("calculate", CultureInfo.CurrentCulture).ConfigureAwait(false);

            if (selections == null)
            {
                result = null;
            }
            else if (selections.IsNullOrEmpty())
            {
                result = new CalculationDto(MessageFactoryRest.GetCalculationResponse(StaticRandom.U1000, StaticRandom.I100));
            }

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:calc:{result.AvailableSelections.Count}"), result, CultureInfo.CurrentCulture, DtoType.Calculation, null).ConfigureAwait(false);
                return new Calculation(result);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<ICalculationFilter> CalculateProbabilityFilteredAsync(IEnumerable<ISelection> selections)
        {
            RecordCall("CalculateProbabilityFilteredAsync");
            var filePath = GetFile("calculate_filter_response.xml", CultureInfo.CurrentCulture);
            var restDeserializer = new Deserializer<FilteredCalculationResponseType>();
            var mapper = new CalculationFilteredMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            await ExecuteDelayAsync("calculate_filter", CultureInfo.CurrentCulture).ConfigureAwait(false);

            if (selections == null)
            {
                result = null;
            }
            else if (selections.IsNullOrEmpty())
            {
                result = new FilteredCalculationDto(MessageFactoryRest.GetFilteredCalculationResponse(StaticRandom.U1000, StaticRandom.I100));
            }

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:calcfilt:{result.AvailableSelections.Count}"), result, CultureInfo.CurrentCulture, DtoType.Calculation, null).ConfigureAwait(false);
                return new CalculationFilter(result);
            }

            return null;
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="System.DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all fixtures that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, URN sportId, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of almost all events we are offering prematch odds for.
        /// </summary>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        public Task<IEnumerable<Tuple<URN, URN>>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of all the available tournaments for a specific sport
        /// </summary>
        /// <param name="sportId">The specific sport id</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the available tournament ids with the sportId it belongs to</returns>
        public Task<IEnumerable<Tuple<URN, URN>>> GetSportAvailableTournamentsAsync(URN sportId, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="System.DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all results that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, URN sportId, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get stage event period summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <param name="competitorIds">The list of competitor ids to fetch the results for</param>
        /// <param name="periods">The list of period ids to fetch the results for</param>
        /// <returns>The periods summary or null if not found</returns>
        public Task<PeriodSummaryDTO> GetPeriodSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester, ICollection<URN> competitorIds = null, ICollection<int> periods = null)
        {
            throw new NotImplementedException();
        }

        private async Task LogSaveDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (item != null)
            {
                var stopWatch = Stopwatch.StartNew();
                await _cacheManager.SaveDtoAsync(id, item, culture, dtoType, requester).ConfigureAwait(false);
                stopWatch.Stop();
                if (stopWatch.ElapsedMilliseconds > 100)
                {
                    Debug.WriteLine($"Saving took {stopWatch.ElapsedMilliseconds} ms. For id={id}, culture={culture.TwoLetterISOLanguageName} and dtoType={dtoType}.");
                }
            }
        }
    }
}
