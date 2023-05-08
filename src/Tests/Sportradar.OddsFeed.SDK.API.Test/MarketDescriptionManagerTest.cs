/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class MarketDescriptionManagerTest
    {
        private IMarketDescriptionCache _variantMarketDescriptionCache;
        private IVariantDescriptionCache _variantDescriptionListCache;
        private IMarketDescriptionCache _invariantMarketDescriptionCache;
        private IMarketCacheProvider _marketCacheProvider;

        private TestDataRouterManager _dataRouterManager;
        private TestProducersProvider _producersProvider;

        [TestInitialize]
        public void Init()
        {
            var variantMarketDescriptionMemoryCache = new MemoryCache("variantMarketDescriptionCache");
            var variantDescriptionMemoryCache = new MemoryCache("variantDescriptionCache");
            var invariantMarketDescriptionMemoryCache = new MemoryCache("invariantMarketDescriptionCache");

            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager);
            _producersProvider = new TestProducersProvider();

            IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

            var timerVdl = new TestTimer(true);
            var timerIdl = new TestTimer(true);
            _variantMarketDescriptionCache = new VariantMarketDescriptionCache(variantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, cacheManager);
            _variantDescriptionListCache = new VariantDescriptionListCache(variantDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, timerVdl, TestData.Cultures, cacheManager);
            _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(invariantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, timerIdl, TestData.Cultures, cacheManager);

            _marketCacheProvider = new MarketCacheProvider(_invariantMarketDescriptionCache, _variantMarketDescriptionCache, _variantDescriptionListCache);
        }

        [TestMethod]
        public void MarketDescriptionManagerInit()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            Assert.IsNotNull(marketDescriptionManager);
        }

        [TestMethod]
        public async Task MarketDescriptionManagerGetMarketDescriptions()
        {
            // calls from initialization are done
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointVariantDescriptions]);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointMarketDescriptions]);
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            var marketDescriptions = (await marketDescriptionManager.GetMarketDescriptionsAsync()).ToList();
            // no new calls should be done, since already everything loaded
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointVariantDescriptions]);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointMarketDescriptions]);
            Assert.AreEqual(TestData.InvariantListCacheCount, marketDescriptions.Count);
        }

        [TestMethod]
        public void MarketDescriptionManagerGetMarketMapping()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            var marketMapping = marketDescriptionManager.GetMarketMappingAsync(115, _producersProvider.GetProducers().First()).GetAwaiter().GetResult().ToList();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointVariantDescriptions]);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointMarketDescriptions]);
            Assert.AreEqual(3, marketMapping.Count);
            Assert.AreEqual("6:14", marketMapping[0].MarketId);
        }
    }
}
