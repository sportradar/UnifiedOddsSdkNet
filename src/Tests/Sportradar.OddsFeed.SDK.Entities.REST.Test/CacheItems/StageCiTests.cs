using System;
using System.Globalization;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test.CacheItems
{
    [TestClass]
    public class StageCiTests
    {
        private readonly TestDataRouterManager _dataRouterManager;
        private readonly ISemaphorePool _semaphorePool;
        private readonly MemoryCache _fixtureTimestampCacheStore;
        private readonly URN _stageId = URN.Parse("sr:stage:123");

        public StageCiTests()
        {
            _fixtureTimestampCacheStore = new MemoryCache("any");
            _semaphorePool = new SemaphorePool(100, ExceptionHandlingStrategy.THROW);

            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager);
        }

        [TestMethod]
        public void ConstructStageFromId()
        {
            var stageCi = new StageCI(_stageId, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(_stageId, stageCi.Id);
            Assert.IsNull(stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void ConstructStageFromStageDTO()
        {
            var stageSummaryEndpoint = MessageFactoryRest.GetStageSummaryEndpoint(123);
            stageSummaryEndpoint.sport_event.id = _stageId.ToString();
            stageSummaryEndpoint.sport_event.stage_type = "race";

            var stageDto = new StageDTO(stageSummaryEndpoint);
            var stageCi = new StageCI(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(stageDto.Id, stageCi.Id);
            Assert.AreEqual(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void ConstructStageFromFixtureDto()
        {
            var fixture = MessageFactoryRest.GetFixture(123);
            fixture.id = _stageId.ToString();
            fixture.stage_type = "race";

            var stageDto = new FixtureDTO(fixture, DateTime.Now);
            var stageCi = new StageCI(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(stageDto.Id, stageCi.Id);
            Assert.AreEqual(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void ConstructStageFromExportableStage()
        {
            var fixture = MessageFactoryRest.GetFixture(123);
            fixture.id = _stageId.ToString();
            fixture.stage_type = "race";

            var stageDto = new FixtureDTO(fixture, DateTime.Now);
            var stageCi = new StageCI(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var exportedStage = (ExportableStageCI)stageCi.ExportAsync().GetAwaiter().GetResult();
            var newStageCi = new StageCI(exportedStage, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            Assert.IsNotNull(newStageCi);
            Assert.AreEqual(stageDto.Id, newStageCi.Id);
            Assert.AreEqual(stageDto.StageType, newStageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void MergeStageToBlankFromId()
        {
            var stageCi = new StageCI(_stageId, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            var fixture = MessageFactoryRest.GetFixture(123);
            fixture.id = _stageId.ToString();
            fixture.stage_type = "race";
            var fixtureDto = new FixtureDTO(fixture, DateTime.Now);

            stageCi.MergeFixture(fixtureDto, CultureInfo.CurrentCulture, true);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(_stageId, stageCi.Id);
            Assert.AreEqual(StageType.Race, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void MergeStageFromStageDTOWithoutStageTypeDoesNotRemovesIt()
        {
            var stageSummaryEndpoint = MessageFactoryRest.GetStageSummaryEndpoint(123);
            stageSummaryEndpoint.sport_event.id = _stageId.ToString();
            stageSummaryEndpoint.sport_event.stage_type = "race";
            var stageDto = new StageDTO(stageSummaryEndpoint);
            var stageCi = new StageCI(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            var stageSummaryEndpoint2 = MessageFactoryRest.GetStageSummaryEndpoint(123);
            stageSummaryEndpoint2.sport_event.id = _stageId.ToString();
            var stageDto2 = new StageDTO(stageSummaryEndpoint2);

            stageCi.Merge(stageDto2, CultureInfo.CurrentCulture, true);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(stageDto.Id, stageCi.Id);
            Assert.AreEqual(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void MergeStageFromFixtureDtoWithoutStageTypeDoesNotRemovesIt()
        {
            var fixture = MessageFactoryRest.GetFixture(123);
            fixture.id = _stageId.ToString();
            fixture.stage_type = "race";
            var stageDto = new FixtureDTO(fixture, DateTime.Now);
            var stageCi = new StageCI(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

            var fixture2 = MessageFactoryRest.GetFixture(123);
            fixture2.id = _stageId.ToString();
            var stageDto2 = new FixtureDTO(fixture2, DateTime.Now);
            stageCi.Merge(stageDto2, CultureInfo.CurrentCulture, true);

            Assert.IsNotNull(stageCi);
            Assert.AreEqual(stageDto.Id, stageCi.Id);
            Assert.AreEqual(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }
    }
}
