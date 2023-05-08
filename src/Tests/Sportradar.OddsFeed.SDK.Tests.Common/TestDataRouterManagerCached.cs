/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
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
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Xunit.Abstractions;

// ReSharper disable UnusedMember.Local

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestDataRouterManagerCached : IDataRouterManager
{
    public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

    /// <inheritdoc />
    public Task GetSportEventSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetSportEventFixtureAsync(URN id, CultureInfo culture, bool useCachedProvider, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetAllTournamentsForAllSportAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetSportCategoriesAsync(URN id, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetAllSportsAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Tuple<URN, URN>>> GetLiveSportEventsAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForDateAsync(DateTime date, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetPlayerProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetCompetitorAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<URN>> GetSeasonsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<MatchTimelineDTO> GetInformationAboutOngoingEventAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetMarketDescriptionsAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetVariantMarketDescriptionAsync(int id, string variant, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetVariantDescriptionsAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetDrawSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetDrawFixtureAsync(URN id, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task GetLotteryScheduleAsync(URN lotteryId, CultureInfo culture, ISportEventCI requester)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Tuple<URN, URN>>> GetAllLotteriesAsync(CultureInfo culture, bool ignoreFail)
    {
        throw new NotImplementedException();
    }

    public const string EndpointSportEventSummary = "GetSportEventSummaryAsync";
    public const string EndpointSportEventFixture = "GetSportEventFixtureAsync";
    public const string EndpointAllTournamentsForAllSport = "GetAllTournamentsForAllSportAsync";
    public const string EndpointSportCategories = "GetSportCategoriesAsync";
    public const string EndpointAllSports = "GetAllSportsAsync";
    public const string EndpointLiveSportEvents = "GetLiveSportEventsAsync";
    public const string EndpointSportEventsForDate = "GetSportEventsForDateAsync";
    public const string EndpointSportEventsForTournament = "GetSportEventsForTournamentAsync";
    public const string EndpointPlayerProfile = "GetPlayerProfileAsync";
    public const string EndpointCompetitor = "GetCompetitorAsync";
    public const string EndpointSeasonsForTournament = "GetSeasonsForTournamentAsync";
    public const string EndpointInformationAboutOngoingEvent = "GetInformationAboutOngoingEventAsync";
    public const string EndpointMarketDescriptions = "GetMarketDescriptionsAsync";
    public const string EndpointVariantMarketDescription = "GetVariantMarketDescriptionAsync";
    public const string EndpointVariantDescriptions = "GetVariantDescriptionsAsync";
    public const string EndpointDrawSummary = "GetDrawSummaryAsync";
    public const string EndpointDrawFixture = "GetDrawFixtureAsync";
    public const string EndpointLotterySchedule = "GetLotteryScheduleAsync";
    public const string EndpointAllLotteries = "GetAllLotteriesAsync";
    public const string EndpointAvailableSelections = "GetAvailableSelectionsAsync";
    public const string EndpointCalculateProbability = "CalculateProbabilityAsync";
    public const string EndpointCalculateProbabilityFiltered = "CalculateProbabilityFilteredAsync";

    private const string FixtureXml = "fixtures_{culture}.xml";
    private const string SportCategoriesXml = "sport_categories_{culture}.xml";
    private const string ScheduleXml = "schedule_{culture}.xml";
    private const string TourScheduleXml = "tournament_schedule_{culture}.xml";
    private const string SportsXml = "sports_{culture}.xml";
    private const string MatchDetailsXml = "event_details_{culture}.xml";
    private const string TournamentScheduleXml = "tournaments_{culture}.xml";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Allowed for future reference")]
    private const string PlayerProfileXml = "player_1_{culture}.xml";

    private const string SimpleTeamProfileXml = "simpleteam_1_{culture}.xml";

    private readonly ICacheManager _cacheManager;
    private readonly IDeserializer<RestMessage> _restDeserializer = new Deserializer<RestMessage>();
    private TimeSpan _delay = TimeSpan.Zero;
    private bool _delayVariable;
    private int _delayPercent = 10;

    /// <summary>
    /// The list of URI replacements (to get wanted response when specific url is called)
    /// </summary>
    public readonly List<Tuple<string, string>> UriReplacements;

    public int TotalRestCalls => RestMethodCalls.Sum(s => s.Value);

    public readonly Dictionary<string, int> RestMethodCalls;

    public readonly List<string> RestUrlCalls;

    private readonly object _lock = new object();

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// The exception handling strategy
    /// </summary>
    public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; internal set; }

    internal TestDataRouterManagerCached(ICacheManager cacheManager, IReadOnlyCollection<CultureInfo> loadCultures, ITestOutputHelper outputHelper)
    {
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _outputHelper = outputHelper;
        UriReplacements = new List<Tuple<string, string>>();
        RestMethodCalls = new Dictionary<string, int>();
        RestUrlCalls = new List<string>();
        ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;
    }

    private string FindUriReplacement(string path, string cultureIso, string defaultPath)
    {
        var replacement = UriReplacements.Where(w => w.Item1.Equals(path)).ToList();
        return replacement.IsNullOrEmpty() ? defaultPath : replacement.First().Item2.Replace("culture", cultureIso);
    }

    private static string GetFile(string template, CultureInfo culture)
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

        return string.Empty;
    }

    private void RecordMethodCall(string callType)
    {
        lock (_lock)
        {
            if (RestMethodCalls.ContainsKey(callType))
            {
                RestMethodCalls.TryGetValue(callType, out var value);
                RestMethodCalls[callType] = value + 1;
            }
            else
            {
                RestMethodCalls.Add(callType, 1);
            }
        }
    }

    private void ResetMethodCall(string callType)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(callType))
            {
                RestMethodCalls.Clear();
                return;
            }

            if (RestMethodCalls.ContainsKey(callType))
            {
                RestMethodCalls[callType] = 0;
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
        lock (_lock)
        {
            if (string.IsNullOrEmpty(callType))
            {
                return TotalRestCalls;
            }

            if (!RestMethodCalls.ContainsKey(callType))
            {
                return 0;
            }

            RestMethodCalls.TryGetValue(callType, out var value);
            return value;
        }
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

            _outputHelper.WriteLine($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs} ms START");
            await Task.Delay(delayMs).ConfigureAwait(false);
            _outputHelper.WriteLine($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs} ms END");
        }
    }


    public async Task<IAvailableSelections> GetAvailableSelectionsAsync(URN id)
    {
        RecordMethodCall(EndpointAvailableSelections);

        await ExecuteDelayAsync("available_selections", CultureInfo.CurrentCulture).ConfigureAwait(false);

        var restDeserializer = new Deserializer<AvailableSelectionsType>();
        var mapper = new AvailableSelectionsMapperFactory();

        var filePath = GetFile("available_selections.xml", CultureInfo.CurrentCulture);
        var stream = FileHelper.OpenFile(filePath);
        var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
        RestUrlCalls.Add(filePath);

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
        RecordMethodCall(EndpointCalculateProbability);

        await ExecuteDelayAsync("calculate", CultureInfo.CurrentCulture).ConfigureAwait(false);

        var restDeserializer = new Deserializer<CalculationResponseType>();
        var mapper = new CalculationMapperFactory();

        var filePath = GetFile("calculate_response.xml", CultureInfo.CurrentCulture);
        var stream = FileHelper.OpenFile(filePath);
        var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
        RestUrlCalls.Add(filePath);

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
            await LogSaveDtoAsync(URN.Parse($"sr:calc:{result.AvailableSelections.Count}"), result, CultureInfo.CurrentCulture, DtoType.Calculation, null)
                .ConfigureAwait(false);
            return new Calculation(result);
        }

        return null;
    }

    public async Task<ICalculationFilter> CalculateProbabilityFilteredAsync(IEnumerable<ISelection> selections)
    {
        RecordMethodCall(EndpointCalculateProbabilityFiltered);

        await ExecuteDelayAsync("calculate_filter", CultureInfo.CurrentCulture).ConfigureAwait(false);

        var restDeserializer = new Deserializer<FilteredCalculationResponseType>();
        var mapper = new CalculationFilteredMapperFactory();

        var filePath = GetFile("calculate_filter_response.xml", CultureInfo.CurrentCulture);
        var stream = FileHelper.OpenFile(filePath);
        var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
        RestUrlCalls.Add(filePath);

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
            await LogSaveDtoAsync(URN.Parse($"sr:calcfilt:{result.AvailableSelections.Count}"), result, CultureInfo.CurrentCulture, DtoType.Calculation, null)
                .ConfigureAwait(false);
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
    public Task<PeriodSummaryDTO> GetPeriodSummaryAsync(URN id,
                                                        CultureInfo culture,
                                                        ISportEventCI requester,
                                                        ICollection<URN> competitorIds = null,
                                                        ICollection<int> periods = null)
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
                _outputHelper.WriteLine($"Saving took {stopWatch.ElapsedMilliseconds} ms. For id={id}, culture={culture.TwoLetterISOLanguageName} and dtoType={dtoType}.");
            }
        }
    }
}