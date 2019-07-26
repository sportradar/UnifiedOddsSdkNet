/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    /// <summary>
    /// For testing functionality of various caches handling missing fetches
    /// </summary>
    [TestClass]
    public class CacheExportTest
    {
        const string AllTournaments = "GetAllTournamentsForAllSportAsync";
        const string AllSports = "GetAllSportsAsync";
        const string SportEventSummary = "GetSportEventSummaryAsync";

        private SportDataCache _sportDataCache;
        private SportEventCache _sportEventCache;
        private MemoryCache _memoryCache;

        private TestTimer _timer;

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("tournamentDetailsCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(_memoryCache, _dataRouterManager, new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);
            _sportDataCache = new SportDataCache(_dataRouterManager, _timer, TestData.Cultures, _sportEventCache, _cacheManager);
        }

        [TestMethod]
        public void SportDataCacheStatusTest()
        {
            var status = _sportDataCache.CacheStatus();
            Assert.AreEqual(0, status["SportCI"]);
            Assert.AreEqual(0, status["CategoryCI"]);


            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).Result; // initial load

            status = _sportDataCache.CacheStatus();
            Assert.AreEqual(TestData.CacheSportCount, status["SportCI"]);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, status["CategoryCI"]);
        }

        [TestMethod]
        public async Task SportDataCacheEmptyExportTest()
        {
            var export = (await _sportDataCache.ExportAsync()).ToList();
            Assert.AreEqual(0, export.Count);

            await _sportDataCache.ImportAsync(export);
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
        }

        [TestMethod]
        public async Task SportDataCacheFullExportTest()
        {
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).Result; // initial load

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");


            var export = (await _sportDataCache.ExportAsync()).ToList();
            Assert.AreEqual(TestData.CacheSportCount + TestData.CacheCategoryCountPlus, export.Count);

            _sportDataCache.Sports.Clear();
            _sportDataCache.Categories.Clear();
            _sportDataCache.FetchedCultures.Clear();
            
            await _sportDataCache.ImportAsync(export);
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

            // No calls to the data router manager
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _sportDataCache.ExportAsync());
            Assert.AreEqual(exportString, secondExportString);
        }

        private string SerializeExportables(IEnumerable<ExportableCI> exportables)
        {
            return JsonConvert.SerializeObject(exportables.OrderBy(e => e.Id));
        }
    }
}
