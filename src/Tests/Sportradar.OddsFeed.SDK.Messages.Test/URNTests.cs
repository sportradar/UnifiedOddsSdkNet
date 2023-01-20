/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sportradar.OddsFeed.SDK.Messages.Test
{
    [TestClass]
    public class UrnTests
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingPrefixIsNotAllowed()
        {
            const string urn = "match:1234";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingTypeIsNotAllowed()
        {
            const string urn = "sr:1234";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void NumberInTypeIsNotAllowed()
        {
            const string urn = "sr:match1:12333";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MinusInTypeIsNotAllowed()
        {
            const string urn = "sr:ma-tch:1232";
            URN.Parse(urn);
        }

        [TestMethod]
        public void UnsupportedTypeIsNotAllowed()
        {
            var urn = URN.Parse("sr:event_tournament:1232");
            Assert.AreEqual(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingIdIsNotAllowed()
        {
            const string urn = "sr:match";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void LetterInIdIsNotAllowed()
        {
            const string urn = "sr:match:12a34";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UnderscoreInIdIsNotAllowed()
        {
            const string urn = "sr:match:123_4";
            URN.Parse(urn);
        }

        [TestMethod]
        public void SportEventResourceIsSupported()
        {
            const string urnString = "sr:sport_event:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.MATCH, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void VfPrefixIsAllowed()
        {
            const string urnString = "vf:match:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
        }

        [TestMethod]
        public void RaceEventResourceIsSupported()
        {
            const string urnString = "sr:race_event:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.STAGE, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void SeasonResourceIsSupported()
        {
            const string urnString = "sr:season:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.SEASON, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void TournamentResourceIsSupported()
        {
            const string urnString = "sr:tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.TOURNAMENT, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void SimpleTournamentResourceIsSupported()
        {
            const string urnString = "sr:simple_tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.BASIC_TOURNAMENT, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void RaceTournamentResourceIsSupported()
        {
            const string urnString = "sr:race_tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.STAGE, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void ParsedUrnHasCorrectValues()
        {
            const string urnString = "sr:sport_event:1234";
            var urn = URN.Parse(urnString);

            Assert.IsNotNull(urn, "urn cannot be a null reference");
            Assert.AreEqual("sr", urn.Prefix, "Value of the prefix is not correct");
            Assert.AreEqual("sport_event", urn.Type, "Value of type is not correct");
            Assert.AreEqual(ResourceTypeGroup.MATCH, urn.TypeGroup, "Value of the typeGroup ils not correct");
            Assert.AreEqual(1234, urn.Id, "Value of the Id is not correct");
        }

        [TestMethod]
        public void UrnWithNegativeId()
        {
            const string urnString = "wns:draw:-2143997118";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.DRAW, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void CustomEventUrn()
        {
            const string urnString = "ccc:match:1234";
            var urn = URN.Parse(urnString);

            Assert.IsNotNull(urn, "urn cannot be a null reference");
            Assert.AreEqual("ccc", urn.Prefix, "Value of the prefix is not correct");
            Assert.AreEqual("match", urn.Type, "Value of type is not correct");
            Assert.AreEqual(ResourceTypeGroup.MATCH, urn.TypeGroup, "Value of the typeGroup ils not correct");
            Assert.AreEqual(1234, urn.Id, "Value of the Id is not correct");
        }

        [TestMethod]
        public void CustomSimpleTournamentEventUrn()
        {
            const string urnString = "ccc:simple_tournament:1234";
            var urn = URN.Parse(urnString);

            Assert.IsNotNull(urn);
            Assert.AreEqual("ccc", urn.Prefix);
            Assert.AreEqual("simple_tournament", urn.Type);
            Assert.AreEqual(ResourceTypeGroup.BASIC_TOURNAMENT, urn.TypeGroup);
            Assert.AreEqual(1234, urn.Id);
        }

        [TestMethod]
        public void SrGroupRoundUrn()
        {
            const string urnString = "sr:group:1234";
            var urn = URN.Parse(urnString);

            Assert.IsNotNull(urn);
            Assert.AreEqual("sr", urn.Prefix);
            Assert.AreEqual("group", urn.Type);
            Assert.AreEqual(ResourceTypeGroup.OTHER, urn.TypeGroup);
            Assert.AreEqual(1234, urn.Id);
        }

        [TestMethod]
        public void ParseCustomTypeUrn()
        {
            const string urnString = "sr:abcde:1234";
            var urn = URN.Parse(urnString, true);

            Assert.IsNotNull(urn);
            Assert.AreEqual("sr", urn.Prefix);
            Assert.AreEqual("abcde", urn.Type);
            Assert.AreEqual(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
            Assert.AreEqual(1234, urn.Id);
        }

        [TestMethod]
        public void TryParseCustomTypeUrn()
        {
            const string urnString = "sr:abcde:1234";
            URN.TryParse(urnString, true, out var urn);

            Assert.IsNotNull(urn);
            Assert.AreEqual("sr", urn.Prefix);
            Assert.AreEqual("abcde", urn.Type);
            Assert.AreEqual(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
            Assert.AreEqual(1234, urn.Id);
        }
    }
}
