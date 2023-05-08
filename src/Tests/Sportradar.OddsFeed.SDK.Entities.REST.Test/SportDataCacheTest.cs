/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    /// <summary>
    /// For testing functionality of <see cref="SportDataCache"/>
    /// <remarks>DataProvider calls GET "All available tournaments for all sports" for all requests</remarks>
    /// </summary>
    [TestClass]
    public class SportDataCacheTest
    {
        private SportDataCache _sportDataCache;
        private ISportEventCache _sportEventCache;
        private MemoryCache _memoryCache;

        private TestTimer _timer;

        private static readonly URN EventId = URN.Parse("sr:match:9210275"); // "sr:match:8629794";
        private static readonly URN PlayerId = URN.Parse("sr:player:9210275");
        private static readonly URN CompetitorId = URN.Parse("sr:competitor:9210275");
        private static readonly URN SportId = URN.Parse("sr:sport:1");
        private static readonly URN TournamentId = URN.Parse("sr:tournament:1");
        private static readonly URN TournamentIdExtra = URN.Parse("sr:simple_tournament:11111");
        private static readonly URN DrawId = URN.Parse("wns:draw:9210275");
        private static readonly URN LotteryId = URN.Parse("wns:lottery:9210275");

        readonly CultureInfo _cultureEn = new CultureInfo("en");
        readonly CultureInfo _cultureNl = new CultureInfo("nl");

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _memoryCache = new MemoryCache("cache");

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(_memoryCache, _dataRouterManager, new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5, ExceptionHandlingStrategy.THROW), _cultureEn, new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);
            _sportDataCache = new SportDataCache(_dataRouterManager, _timer, TestData.Cultures, _sportEventCache, _cacheManager);
        }

        [TestMethod]
        public void TestDataRouterManagerIsCalledOneTime()
        {
            const string callType = "GetSportEventSummaryAsync";
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));
            _dataRouterManager.GetSportEventSummaryAsync(TournamentIdExtra, _cultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly 1 time.");
        }

        [TestMethod]
        public void TestDataRouterManagerAllMethods()
        {
            var callType = string.Empty;
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));
            _dataRouterManager.GetSportEventSummaryAsync(EventId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetSportEventFixtureAsync(EventId, _cultureEn, true, null).ConfigureAwait(false);
            _dataRouterManager.GetAllTournamentsForAllSportAsync(_cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetAllSportsAsync(_cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetLiveSportEventsAsync(_cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetSportEventsForDateAsync(DateTime.Now, _cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetSportEventsForTournamentAsync(TournamentIdExtra, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetPlayerProfileAsync(PlayerId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetCompetitorAsync(CompetitorId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetSeasonsForTournamentAsync(TournamentIdExtra, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetInformationAboutOngoingEventAsync(EventId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetMarketDescriptionsAsync(_cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetVariantDescriptionsAsync(_cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetVariantMarketDescriptionAsync(1, "variant", _cultureEn).ConfigureAwait(false);
            _dataRouterManager.GetDrawSummaryAsync(DrawId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetDrawFixtureAsync(DrawId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetLotteryScheduleAsync(LotteryId, _cultureEn, null).ConfigureAwait(false);
            _dataRouterManager.GetAllLotteriesAsync(_cultureEn, false).ConfigureAwait(false);
            Assert.AreEqual(18, _dataRouterManager.GetCallCount(callType), "DataRouterManager should be called exactly 18 times.");
        }

        [TestMethod]
        public void TestDataIsCaching()
        {
            const string callType = "GetAllSportsAsync";
            _timer.FireOnce(TimeSpan.Zero);
            Task.Delay(500).GetAwaiter().GetResult();

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult();
            Assert.IsNotNull(sports, "Retrieved sports cannot be null");
            Assert.IsTrue(sports.Any(), "sports.Count() > 0");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void TestDataIsCached()
        {
            const string callType = "GetAllSportsAsync";
            var allSports = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult();
            Assert.IsNotNull(allSports, "List of sports cannot be a null reference");
            Assert.AreEqual(136, allSports.Count(), "The number of sports must be 136.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
            Task.Run(async () =>
            {
                _timer.FireOnce(TimeSpan.Zero);

                var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false);
                Assert.IsNotNull(sports, "Retrieved sports cannot be null");

                var sport = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures).ConfigureAwait(false);
                Assert.IsNotNull(sport, "sport cannot be a null reference");

                var tournamentSport = await _sportDataCache.GetSportForTournamentAsync(URN.Parse("sr:tournament:146"), TestData.Cultures);
                Assert.IsNotNull(tournamentSport, "tournamentSport cannot be a null reference");
                Assert.AreEqual(1, tournamentSport.Categories.Count(), "The number of categories must be 1");
                Assert.AreEqual(1, tournamentSport.Categories.First().Tournaments.Count(), "the number of tournaments must be 1");
            }).GetAwaiter().GetResult();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public async Task TimerCalledMultipleTimesInvokesApiCalls()
        {
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllLotteries));

            const int nbrTimerCalls = 3;
            for (var i = 0; i < nbrTimerCalls; i++)
            {
                _timer.FireOnce(TimeSpan.Zero);
            }
            var sport = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures);
            Assert.IsNotNull(sport);
            Assert.AreEqual(TestData.Cultures.Count, sport.Names.Count);

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures);
            Assert.IsNotNull(sports);
            Assert.AreEqual(136, sports.Count());
            Assert.AreEqual(TestData.Cultures.Count * nbrTimerCalls, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.AreEqual(TestData.Cultures.Count * nbrTimerCalls, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.AreEqual(TestData.Cultures.Count * nbrTimerCalls, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllLotteries));
            Assert.AreEqual(TestData.Cultures.Count, _sportDataCache.FetchedCultures.Count);
        }

        [TestMethod]
        public void ConcurrencyFetch()
        {
            IEnumerable<SportData> sports = null;
            SportData sport = null;
            SportData tournamentSport = null;
            TournamentInfoCI tournament = null;
            IEnumerable<Tuple<URN, URN>> tournamentEvents = null;
            IEnumerable<Tuple<URN, URN>> dateEvents = null;

            for (var i = 0; i < 5; i++)
            {
                Task.Run(async () =>
                {
                    _sportDataCache.FetchedCultures.Clear();
                    sports = await _sportDataCache.GetSportsAsync(TestData.Cultures);
                    sport = await _sportDataCache.GetSportAsync(TestData.SportId, TestData.Cultures);
                    tournamentSport = await _sportDataCache.GetSportForTournamentAsync(URN.Parse("sr:tournament:146"), TestData.Cultures);

                    tournamentEvents = await _sportEventCache.GetEventIdsAsync(TestData.TournamentId, _cultureEn);
                    tournament = (TournamentInfoCI)_sportEventCache.GetEventCacheItem(TestData.TournamentId);
                    dateEvents = await _sportEventCache.GetEventIdsAsync(DateTime.Now, _cultureEn);
                }).GetAwaiter().GetResult();

                Assert.IsNotNull(sports, "Retrieved sports cannot be null");
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var s in sports)
                {
                    BaseSportDataValidation(s);
                }
                Assert.IsNotNull(sport, "sport cannot be a null reference");
                Assert.IsNotNull(tournament, "tournament cannot be a null reference");
                Assert.IsNotNull(tournamentEvents, "tournamentEvents cannot be a null reference");
                Assert.IsNotNull(dateEvents, "dateEvents cannot be a null reference");
                Assert.AreEqual(tournamentSport.Categories.Count(), 1, "The number of categories must be 1");
                Assert.AreEqual(tournamentSport.Categories.First().Tournaments.Count(), 1, "the number of tournaments must be 1");
            }
        }

        [TestMethod]
        public void CacheFetchesOnlyOnceStartedByTimer()
        {
            const string callType = "GetAllSportsAsync";
            Task.Run(async () =>
            {
                _timer.FireOnce(TimeSpan.Zero);
                await _sportDataCache.GetSportsAsync(TestData.Cultures);
                await _sportDataCache.GetSportsAsync(TestData.Cultures);
                await _sportDataCache.GetSportsAsync(TestData.Cultures);
            }).GetAwaiter().GetResult();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void CacheFetchesOnlyOnceStartedManually()
        {
            const string callType = "GetAllSportsAsync";
            Task.Run(async () =>
            {
                Task t = _sportDataCache.GetSportsAsync(TestData.Cultures);
                await Task.Delay(100);
                _timer.FireOnce(TimeSpan.Zero);
                await t;
            }).GetAwaiter().GetResult();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void GetSportsAsyncReturnsCorrectlyForBaseLocale()
        {
            List<SportData> sports = null;
            Task.Run(async () =>
            {
                var sprts = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
                sports = sprts.ToList();
            }).GetAwaiter().GetResult();

            foreach (var s in sports)
            {
                BaseSportDataValidation(s);
            }
        }

        [TestMethod]
        public void GetSportsAsyncReturnsCorrectlyForNewLocale()
        {
            List<SportData> sports = null;
            List<SportData> sportsNl = null;
            Task.Run(async () =>
            {
                await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
                var sprts = await _sportDataCache.GetSportsAsync(new[] { _cultureNl }).ConfigureAwait(false);
                sportsNl = sprts.ToList();
                sprts = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false);
                sports = sprts.ToList();
            }).GetAwaiter().GetResult();

            foreach (var s in sportsNl)
            {
                BaseSportDataValidation(s, 1);
            }

            foreach (var s in sports)
            {
                BaseSportDataValidation(s, TestData.Cultures.Count);
            }
        }

        [TestMethod]
        public void GetSportAsyncReturnKnownId()
        {
            const string callType = "GetAllSportsAsync";
            SportData data = null;
            List<SportData> sportsList = null;
            Task.Run(async () =>
            {
                var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
                sportsList = sports.ToList();
                data = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(SportId, data.Id);
            Assert.AreEqual("Soccer", data.Names[_cultureEn]);
            BaseSportDataValidation(data);

            foreach (var s in sportsList)
            {
                BaseSportDataValidation(s);
            }
        }

        [TestMethod]
        public void GetSportAsyncReturnKnownId2()
        {
            const string callType = "GetAllSportsAsync";
            var sportsList = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult(); // initial load
            var data = _sportDataCache.GetSportAsync(SportId, TestData.Cultures).GetAwaiter().GetResult();

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(SportId, data.Id);
            Assert.AreEqual("Soccer", data.Names[_cultureEn]);
            BaseSportDataValidation(data);

            foreach (var s in sportsList)
            {
                BaseSportDataValidation(s);
            }
        }

        [TestMethod]
        public void GetSportAsyncReturnUnknownId()
        {
            var sportId = URN.Parse("sr:sport:11111111");
            SportData data = null;
            Task.Run(async () =>
            {
                data = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures);
            }).GetAwaiter().GetResult();

            Assert.IsNull(data);
        }

        [TestMethod]
        public void GetSportAsyncReturnNewLocale()
        {
            //loads tournaments.xml for new locale
            SportData dataNl = null; // with NL locale
            SportData data01 = null; // without locale
            Task.Run(async () =>
            {
                await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
                dataNl = await _sportDataCache.GetSportAsync(SportId, new[] { _cultureNl }); // add new locale
                data01 = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures); // get all translations
            }).GetAwaiter().GetResult();

            Assert.IsNotNull(dataNl);
            Assert.AreEqual(SportId, dataNl.Id);
            BaseSportDataValidation(dataNl, 1);

            Assert.IsNotNull(dataNl.Categories);
            Assert.IsNotNull(dataNl.Categories.FirstOrDefault()?.Tournaments);
            Assert.AreEqual(@"Voetbal", dataNl.Names[_cultureNl]);
            Assert.AreEqual(@"Internationaal", dataNl.Categories.FirstOrDefault()?.Names[_cultureNl]);
            Assert.AreEqual(URN.Parse("sr:tournament:1"), dataNl.Categories.FirstOrDefault()?.Tournaments.FirstOrDefault());

            BaseSportDataValidation(data01, 3);
        }

        [TestMethod]
        public void GetSportForTournamentAsyncReturnNewLocale()
        {
            //loads tournaments.xml for new locale
            SportData dataNl = null; // with NL locale
            SportData data01 = null; // without locale
            Task.Run(async () =>
            {
                await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
                dataNl = await _sportDataCache.GetSportForTournamentAsync(TournamentId, new[] { _cultureNl }).ConfigureAwait(false); //add new locale
                data01 = await _sportDataCache.GetSportForTournamentAsync(TournamentId, TestData.Cultures).ConfigureAwait(false); // get all translations
            }).GetAwaiter().GetResult();

            Assert.IsNotNull(dataNl);
            Assert.AreEqual(SportId, dataNl.Id);
            BaseSportDataValidation(dataNl, 1);

            Assert.IsNotNull(dataNl.Categories);
            Assert.IsNotNull(dataNl.Categories.FirstOrDefault()?.Tournaments);
            Assert.AreEqual(@"Voetbal", dataNl.Names[_cultureNl]);
            Assert.AreEqual(@"Internationaal", dataNl.Categories.FirstOrDefault()?.Names[_cultureNl]);
            Assert.AreEqual(URN.Parse("sr:tournament:1"), dataNl.Categories.FirstOrDefault()?.Tournaments.FirstOrDefault());

            BaseSportDataValidation(data01, TestData.Cultures.Count);
        }

        [TestMethod]
        public void GetSportForTournamentAsyncReturnForUnknownTournamentId()
        {
            const string callType = "GetAllSportsAsync";
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly 0 times.");

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult(); // initial load
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"), "GetSportEventSummaryAsync should be called exactly 0 times.");
            var data01 = _sportDataCache.GetSportForTournamentAsync(TournamentIdExtra, TestData.Cultures).GetAwaiter().GetResult();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"), $"GetSportEventSummaryAsync should be called exactly {TestData.Cultures.Count} times.");

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);

            Assert.IsNotNull(sports);
            Assert.IsNotNull(data01);
            BaseSportDataValidation(data01, TestData.Cultures.Count);
        }

        [TestMethod]
        public async Task RetrievingNonExistingSport()
        {
            const string callType = "GetAllSportsAsync";
            var sportId = URN.Parse("sr:sport:12345");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures); // initial load
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));

            Assert.IsNull(sportData);
        }

        [TestMethod]
        public async Task RetrievingNonExistingCategory()
        {
            const string callType = "GetAllSportsAsync";
            var categoryId = URN.Parse("sr:category:12345");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures); // initial load
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));

            Assert.IsNull(categoryData);
        }

        [TestMethod]
        public async Task AddNewSportWithCategoryFromSummary()
        {
            const string callType = "GetAllSportsAsync";
            var eventId = URN.Parse("sr:match:123456");
            var sportId = URN.Parse("sr:sport:1888");
            var categoryId = URN.Parse("sr:category:204");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1); // initial load
            Assert.AreEqual(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.IsNull(sportData);

            // adding from summary
            var sportEvent = _sportEventCache.GetEventCacheItem(eventId);
            Assert.IsNotNull(sportEvent);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.AreEqual(sportId, sportEvent.GetSportIdAsync().GetAwaiter().GetResult());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.IsNotNull(sportData);
            Assert.AreEqual(sportId, sportData.Id);
            Assert.IsNotNull(sportData.Categories);
            Assert.AreEqual(1, sportData.Categories.Count());
            Assert.IsTrue(sportData.Categories.Select(s => s.Id).Contains(categoryId));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.IsNotNull(categoryData);
            Assert.AreEqual(categoryId, categoryData.Id);
            Assert.AreEqual(1, categoryData.Names.Count);
        }

        [TestMethod]
        public async Task AddNewCategoryForExistingSportSummary()
        {
            const string callType = "GetAllSportsAsync";
            var eventId = URN.Parse("sr:match:654321");
            var sportId = URN.Parse("sr:sport:18");
            var categoryId = URN.Parse("sr:category:204");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1); // initial load
            Assert.AreEqual(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.IsNotNull(sportData);
            Assert.AreEqual(0, sportData.Categories.Count());

            // adding from summary
            var sportEvent = _sportEventCache.GetEventCacheItem(eventId);
            Assert.IsNotNull(sportEvent);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.AreEqual(sportId, sportEvent.GetSportIdAsync().GetAwaiter().GetResult());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.IsNotNull(sportData);
            Assert.AreEqual(sportId, sportData.Id);
            Assert.IsNotNull(sportData.Categories);
            Assert.AreEqual(1, sportData.Categories.Count());
            Assert.IsTrue(sportData.Categories.Select(s => s.Id).Contains(categoryId));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.IsNotNull(categoryData);
            Assert.AreEqual(categoryId, categoryData.Id);
            Assert.AreEqual(1, categoryData.Names.Count);
        }

        [TestMethod]
        public async Task AddNewCategoryFromCategoryDto()
        {
            const string callType = "GetAllSportsAsync";
            var sportId = URN.Parse("sr:sport:1234");
            var categoryId = URN.Parse("sr:category:4321");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.AreEqual(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.IsNull(sportData);

            // adding from CategoryDto
            var category = MessageFactoryRest.GetCategory((int)categoryId.Id);
            var categoryDto = new CategoryDTO(category.id, category.name, category.country_code, new List<tournamentExtended>());
            await _cacheManager.SaveDtoAsync(categoryDto.Id, categoryDto, TestData.Cultures1.First(), DtoType.Category, null);

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.IsNull(sportData);

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.IsNotNull(categoryData);
            Assert.AreEqual(categoryId, categoryData.Id);
            Assert.AreEqual(1, categoryData.Names.Count);
        }

        [TestMethod]
        public async Task AddNewSportWithCategoryFromCategoryDto()
        {
            const string callType = "GetAllSportsAsync";
            var sportId = URN.Parse("sr:sport:1234");
            var categoryId = URN.Parse("sr:category:4321");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.AreEqual(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.IsNull(sportData);

            // adding from CategoryDto
            var category = MessageFactoryRest.GetCategory((int)categoryId.Id);
            var sport = MessageFactoryRest.GetSport((int)sportId.Id);
            var categoryDto = new CategoryDTO(category.id, category.name, category.country_code, new List<tournamentExtended>());
            var sportDto = new SportDTO(sport.id, sport.name, new List<CategoryDTO> { categoryDto });
            await _cacheManager.SaveDtoAsync(sportDto.Id, sportDto, TestData.Cultures1.First(), DtoType.Sport, null);

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.IsNotNull(sportData);
            Assert.AreEqual(sportId, sportData.Id);
            Assert.IsNotNull(sportData.Categories);
            Assert.AreEqual(1, sportData.Categories.Count());
            Assert.IsTrue(sportData.Categories.Select(s => s.Id).Contains(categoryId));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.IsNotNull(categoryData);
            Assert.AreEqual(categoryId, categoryData.Id);
            Assert.AreEqual(1, categoryData.Names.Count);
        }

        [TestMethod]
        public async Task AddNewSportWithCategoryFromTournament()
        {
            const string callType = "GetAllSportsAsync";
            var sportId = URN.Parse("sr:sport:1234");
            var categoryId = URN.Parse("sr:category:4321");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.AreEqual(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.IsNull(sportData);

            // adding 
            var tournament = MessageFactoryRest.GetTournament();
            tournament.category.id = categoryId.ToString();
            tournament.sport.id = sportId.ToString();
            var tournamentDto = new TournamentDTO(tournament);
            await _cacheManager.SaveDtoAsync(tournamentDto.Id, tournamentDto, TestData.Cultures1.First(), DtoType.Tournament, null);

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.IsNotNull(sportData);
            Assert.AreEqual(sportId, sportData.Id);
            Assert.IsNotNull(sportData.Categories);
            Assert.AreEqual(1, sportData.Categories.Count());
            Assert.IsTrue(sportData.Categories.Select(s => s.Id).Contains(categoryId));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.IsNotNull(categoryData);
            Assert.AreEqual(categoryId, categoryData.Id);
            Assert.AreEqual(1, categoryData.Names.Count);
        }

        [TestMethod]
        public async Task AddNewCategoryForExistingSportFromTournament()
        {
            const string callType = "GetAllSportsAsync";
            var sportId = URN.Parse("sr:sport:18");
            var categoryId = URN.Parse("sr:category:204");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1); // initial load
            Assert.AreEqual(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(callType));
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.IsNotNull(sportData);
            Assert.IsFalse(sportData.Categories.Any());

            // adding
            var tournament = MessageFactoryRest.GetTournament();
            tournament.category.id = categoryId.ToString();
            tournament.sport.id = sportId.ToString();
            var tournamentDto = new TournamentDTO(tournament);
            await _cacheManager.SaveDtoAsync(tournamentDto.Id, tournamentDto, TestData.Cultures1.First(), DtoType.Tournament, null);

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.IsNotNull(sportData);
            Assert.AreEqual(sportId, sportData.Id);
            Assert.IsNotNull(sportData.Categories);
            Assert.AreEqual(1, sportData.Categories.Count());
            Assert.IsTrue(sportData.Categories.Select(s => s.Id).Contains(categoryId));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.IsNotNull(categoryData);
            Assert.AreEqual(categoryId, categoryData.Id);
            Assert.AreEqual(1, categoryData.Names.Count);
        }

        private void BaseSportDataValidation(SportData data)
        {
            BaseSportDataValidation(data, TestData.Cultures.Count);
        }

        private static void BaseSportDataValidation(SportData data, int cultureNbr)
        {
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.Names);

            Assert.AreEqual(cultureNbr, data.Names.Count);

            if (data.Categories != null)
            {
                foreach (var i in data.Categories)
                {
                    Assert.IsNotNull(i.Id);
                    Assert.AreEqual(cultureNbr, i.Names.Count);

                    foreach (var j in i.Tournaments)
                    {
                        Assert.IsNotNull(j.Id);
                    }
                }
            }
        }
    }
}
