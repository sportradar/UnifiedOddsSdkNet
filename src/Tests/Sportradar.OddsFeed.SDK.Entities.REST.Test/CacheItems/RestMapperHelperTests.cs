using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test.CacheItems
{
    [TestClass]
    public class RestMapperHelperTests
    {
        [TestMethod]

        public void AllStageTypeValidInputsAreConvertedToCorrectEnumValue()
        {
            var possibleValues = new Dictionary<string, StageType>()
            {
                { "parent", StageType.Parent },
                { "child", StageType.Child },
                { "event", StageType.Event },
                { "season", StageType.Season },
                { "round", StageType.Round },
                { "competition_group", StageType.CompetitionGroup },
                { "discipline", StageType.Discipline },
                { "dicipline", StageType.Discipline },
                { "race", StageType.Race },
                { "stage", StageType.Stage },
                { "practice", StageType.Practice },
                { "qualifying", StageType.Qualifying },
                { "qualifying_part", StageType.QualifyingPart },
                { "lap", StageType.Lap },
                { "prologue", StageType.Prologue },
                { "run", StageType.Run },
                { "sprint_race", StageType.SprintRace },
            };

            foreach (var possibleValue in possibleValues)
            {
                StringStageTypeIsConvertedToEnum(possibleValue.Key, possibleValue.Value);
            }
        }

        private void StringStageTypeIsConvertedToEnum(string stageTypeString, StageType expectedStageType)
        {
            var result = RestMapperHelper.TryGetStageType(stageTypeString, out var convertedStageType);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedStageType, convertedStageType);
        }

        [TestMethod]
        public void UnknownStageTypeReturnsNull()
        {
            var result = RestMapperHelper.TryGetStageType("unknown", out var convertedStageType);

            Assert.IsFalse(result);
            Assert.IsFalse(convertedStageType.HasValue);
        }

        [TestMethod]
        public void SpringRaceIsConvertedToEnum()
        {
            var result = RestMapperHelper.TryGetStageType("sprint_race", out var convertedStageType);

            Assert.IsTrue(result);
            Assert.AreEqual(StageType.SprintRace, convertedStageType.Value);
        }
    }
}
