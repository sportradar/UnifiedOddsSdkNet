/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class ProfileCacheTest
    {
        private TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;

        [TestInitialize]
        public void Init()
        {
            _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(ScheduleData.Cultures3.ToList());
        }

        private static URN CreatePlayerUrn(int playerId)
        {
            return new URN("sr", "player", playerId);
        }

        private static URN CreateCompetitorUrn(int competitorId)
        {
            return new URN("sr", "competitor", competitorId);
        }

        [TestMethod]
        public void PlayerProfileGetsCached()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            //if we call again, should not fetch again
            player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void PlayerProfileFetchesOnlyMissingCultures()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), new[] { TestData.Culture }, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            //if we call again, should fetch only missing cultures
            player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void NumberOfPlayerProfileEndpointCallsIsCorrect()
        {
            var player1 = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(player1);
            ValidatePlayer(player1, TestData.Culture);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var player2 = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(2), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player2, TestData.Culture);

            Assert.AreNotEqual(player1.Id, player2.Id);
            Assert.AreEqual(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void PreloadedFromCompetitorPlayerProfileEndpointCallsIsCorrect()
        {
            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(player);
            Assert.AreEqual("Smithies, Alex", player.GetName(TestData.Culture));
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void PartiallyPreloadedFromCompetitorPlayerProfileEndpointCallsCompetitorProfileInsteadOfPlayers()
        {
            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), new[] { TestData.Culture }, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(player);
            Assert.AreEqual("Smithies, Alex", player.GetName(TestData.Culture));
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void CompetitorProfileGetsCached()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();

            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            //if we call again, should not fetch again
            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void CompetitorProfileFetchesOnlyMissingCultures()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), new[] { TestData.Culture }, true).GetAwaiter().GetResult();

            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            //if we call again, should not fetch again
            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void NumberOfCompetitorProfileProviderCallsMatchIsCorrect()
        {
            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames1);

            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var competitorNames2 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames2);

            Assert.AreNotEqual(competitorNames1.Id, competitorNames2.Id);
            Assert.AreEqual(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void PreloadedFromMatchCompetitorProfileProviderCallsIsCorrect()
        {
            var matchId = URN.Parse("sr:match:1");
            var cultures = TestData.Cultures3.ToList();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[0], null).GetAwaiter().GetResult();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[1], null).GetAwaiter().GetResult();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[2], null).GetAwaiter().GetResult();
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames1);
            Assert.AreEqual(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void PartiallyPreloadedFromMatchCompetitorProfileProviderCallsIsCorrect()
        {
            var matchId = URN.Parse("sr:match:1");
            var cultures = TestData.Cultures3.ToList();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[0], null).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames1);
            Assert.AreEqual(TestData.Cultures.Count + 1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void MissingPlayerNameForAllCulturesWhenSetToFetchIfMissingFetchesMissingPlayerProfiles()
        {
            var cultures = TestData.Cultures3.ToList();
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[cultures[0]]);
            Assert.AreNotEqual(string.Empty, playerNames[cultures[1]]);
            Assert.AreNotEqual(string.Empty, playerNames[cultures[2]]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void MissingPlayerNameForOneCultureWhenSetToFetchIfMissingFetchesOneMissingPlayerProfile()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures1.First()]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void MissingPlayerNameForAllCulturesWhenSetNotToFetchIfMissingDoesNotFetchesMissingPlayerProfiles()
        {
            var cultures = TestData.Cultures3.ToList();
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.IsTrue(playerNames.All(a => string.IsNullOrEmpty(a.Value)));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void MissingPlayerNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingPlayerProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void PopulatedFromCompetitorMissingPlayerNameForAllCultureWhenSetToFetchIfMissingFetchesMissingCompetitorProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void PopulatedFromCompetitorMissingPlayerNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void MissingPlayerNameForAllCultureWhenSetNotToFetchIfMissingDoesNotOverridePlayerProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreNotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void MissingCompetitorNameForAllCulturesWhenSetToFetchIfMissingFetchesMissingCompetitorProfiles()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures3.First()]);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(TestData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(TestData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void MissingCompetitorNameForOneCultureWhenSetToFetchIfMissingFetchesOneMissingCompetitorProfile()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures1.First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void MissingCompetitorNameForAllCulturesWhenSetNotToFetchIfMissingDoesNotFetchesMissingCompetitorProfiles()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures3, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.IsTrue(competitorNames.All(a => string.IsNullOrEmpty(a.Value)));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void MissingCompetitorNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingCompetitorProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, competitorNames[TestData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[TestData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void PopulatedFromMatchMissingCompetitorNameForAllCultureWhenSetToFetchIfMissingFetchesMissingSummaryEndpoint()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(URN.Parse("sr:match:1"), TestData.Culture, null).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void PopulatedFromMatchMissingCompetitorNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingCompetitorProfile()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(URN.Parse("sr:match:1"), TestData.Culture, null).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.AreEqual(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void MissingCompetitorNameForAllCultureWhenSetNotToFetchIfMissingDoesNotOverrideProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.AreEqual(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.AreNotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        private static void ValidatePlayer(PlayerProfileCI player, CultureInfo culture)
        {
            Assert.IsNotNull(player);
            if (player.Id.Id == 1)
            {
                Assert.AreEqual("van Persie, Robin", player.GetName(culture));
                Assert.AreEqual("Netherlands", player.GetNationality(culture));
                Assert.IsNull(player.Gender);
                Assert.AreEqual("forward", player.Type);
                Assert.AreEqual(183, player.Height);
                Assert.AreEqual(71, player.Weight);
                Assert.AreEqual("NLD", player.CountryCode);
                Assert.IsNotNull(player.DateOfBirth);
                Assert.AreEqual("1983-08-06", player.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
            else if (player.Id.Id == 2)
            {
                Assert.AreEqual("Cole, Ashley", player.GetName(culture));
                Assert.AreEqual("England", player.GetNationality(culture));
                Assert.IsNull(player.Gender);
                Assert.AreEqual("defender", player.Type);
                Assert.AreEqual(176, player.Height);
                Assert.AreEqual(67, player.Weight);
                Assert.AreEqual("ENG", player.CountryCode);
                Assert.IsNotNull(player.DateOfBirth);
                Assert.AreEqual("1980-12-20", player.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
        }
    }
}
