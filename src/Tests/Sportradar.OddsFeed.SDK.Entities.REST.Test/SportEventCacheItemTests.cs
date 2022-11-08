/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Test.Shared;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SportEventCacheItemTests
    {
        const string SportEventSummary = "GetSportEventSummaryAsync";
        const string SportEventFixture = "GetSportEventFixtureAsync";

        private SportEventCache _sportEventCache;
        private MemoryCache _memoryCache;
        private TestTimer _timer;

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("sportEventCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(_memoryCache, _dataRouterManager, new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5, ExceptionHandlingStrategy.THROW), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);
        }

        [TestMethod]
        public async Task FixtureProviderIsCalledOnlyOnceForEachLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetBookingStatusAsync();
            await cacheItem.GetScheduledAsync();
            await cacheItem.GetScheduledEndAsync();
            await cacheItem.GetCompetitorsIdsAsync(TestData.Cultures);
            await cacheItem.GetTournamentRoundAsync(TestData.Cultures);
            await cacheItem.GetSeasonAsync(TestData.Cultures);
            await cacheItem.GetTournamentIdAsync(TestData.Cultures);
            await cacheItem.GetVenueAsync(TestData.Cultures);
            await cacheItem.GetFixtureAsync(TestData.Cultures);
            await cacheItem.GetReferenceIdsAsync();

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task DetailsProviderIsCalledOnlyOnceForEachLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
            var cultures = new[] { new CultureInfo("en") };

            await cacheItem.GetConditionsAsync(cultures);
            await cacheItem.GetConditionsAsync(cultures);
            await cacheItem.GetConditionsAsync(cultures);

            Assert.AreEqual(cultures.Length, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task GetBookingStatusCallsProviderWithDefaultLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetBookingStatusAsync();
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task GetBookingStatusCallsProviderCallsOnlyOnce()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetBookingStatusAsync();
            await cacheItem.GetVenueAsync(TestData.Cultures);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task GetBookingStatusForStageCallsProviderCallsOnlyOnce()
        {
            var cacheItem = (IStageCI)_sportEventCache.GetEventCacheItem(TestData.EventStageId);

            await cacheItem.GetBookingStatusAsync();

            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task GetBookingStatusForStageCallsProviderCallsOnlyOnceRepeated()
        {
            var cacheItem = (IStageCI)_sportEventCache.GetEventCacheItem(TestData.EventStageId);

            await cacheItem.GetBookingStatusAsync();

            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture));

            await cacheItem.GetBookingStatusAsync();

            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task GetScheduleAsyncCallsProviderWithDefaultLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetScheduledAsync();
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task get_schedule_end_async_calls_provider_with_default_locale()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetScheduledEndAsync();
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task GetTournamentIdAsyncCallsProviderWithAllLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetTournamentIdAsync(TestData.Cultures);
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.AreEqual(3, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task NumberOfCallsToFixtureProviderIsEqualToNumberOfLocalsWhenAccessingTheSameProperty()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetVenueAsync(TestData.Cultures);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task NumberOfCallsToSummaryProviderIsEqualNumberOfLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("hu") });

            Assert.AreEqual(3, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task NumberOfCallsToSummaryProviderForNonTranslatableProperty()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.FetchSportEventStatusAsync();
            await cacheItem.FetchSportEventStatusAsync();
            await cacheItem.FetchSportEventStatusAsync();

            Assert.AreEqual(3, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [TestMethod]
        public async Task NumberOfCallsToFixtureProviderIsEqualToNumberOfLocalsWhenAccessingDifferentProperties()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
            await cacheItem.GetFixtureAsync(new[] { new CultureInfo("de") });
            await cacheItem.GetTournamentRoundAsync(new[] { new CultureInfo("hu") });

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }
    }
}
