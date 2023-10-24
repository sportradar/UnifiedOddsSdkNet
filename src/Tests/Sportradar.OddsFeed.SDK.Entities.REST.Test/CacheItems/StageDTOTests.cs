using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test.CacheItems
{
    [TestClass]
    public class StageDTOTests
    {
        [TestMethod]
        public void ConstructStageDTOFromApiStageSportEvent()
        {
            var sportEvent = MessageFactoryRest.GetSportEventEndpoint(123);
            sportEvent.id = "sr:stage:123";
            sportEvent.stage_type = "race";

            var stageDto = new StageDTO(sportEvent);

            Assert.IsNotNull(stageDto);
            Assert.AreEqual(sportEvent.id, stageDto.Id.ToString());
            Assert.AreEqual(StageType.Race, stageDto.StageType);
        }

        [TestMethod]
        public void ConstructStageDTOFromApiStageSummary()
        {
            var stageSummaryEndpoint = MessageFactoryRest.GetStageSummaryEndpoint(123);
            stageSummaryEndpoint.sport_event.id = "sr:stage:123";
            stageSummaryEndpoint.sport_event.stage_type = "race";

            var stageDto = new StageDTO(stageSummaryEndpoint);

            Assert.IsNotNull(stageDto);
            Assert.AreEqual(stageSummaryEndpoint.sport_event.id, stageDto.Id.ToString());
            Assert.AreEqual(StageType.Race, stageDto.StageType);
        }

        [TestMethod]
        public void ConstructStageDTOFromApiParentStage()
        {
            var stageSummaryEndpoint = MessageFactoryRest.GetParentStageSummaryEndpoint(123);
            stageSummaryEndpoint.id = "sr:stage:123";
            stageSummaryEndpoint.stage_type = "parent";

            var stageDto = new StageDTO(stageSummaryEndpoint);

            Assert.IsNotNull(stageDto);
            Assert.AreEqual(stageSummaryEndpoint.id, stageDto.Id.ToString());
            Assert.AreEqual(StageType.Parent, stageDto.StageType);
        }

        [TestMethod]
        public void ConstructStageDTOFromApiSportEventChildrenSportEvent()
        {
            var childStage = new sportEventChildrenSport_event()
            {
                id = "sr:stage:123",
                scheduledSpecified = true,
                scheduled = new DateTime(DateTime.Now.Year, 2, 17),
                scheduled_endSpecified = true,
                scheduled_end = new DateTime(DateTime.Now.Year, 2, 18)
            };
            childStage.id = "sr:stage:123";
            childStage.stage_type = "child";

            var stageDto = new StageDTO(childStage);

            Assert.IsNotNull(stageDto);
            Assert.AreEqual(childStage.id, stageDto.Id.ToString());
            Assert.AreEqual(StageType.Child, stageDto.StageType);
        }
    }
}
