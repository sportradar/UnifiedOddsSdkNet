/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class ProfileCacheSaveTests
    {
        private ScheduleData _scheduleData;
        private TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;

        [TestInitialize]
        public void Init()
        {
            _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(ScheduleData.Cultures3);
            _scheduleData = new ScheduleData(new TestSportEntityFactoryBuilder(ScheduleData.Cultures3));
        }


        private static URN CreateSimpleTeamUrn(int competitorId)
        {
            return new URN("sr", "simple_team", competitorId);
        }

        [TestMethod]
        public void SimpleTeamProfileGetsCached()
        {
            Assert.IsNotNull(_sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), ScheduleData.Cultures3, true).GetAwaiter().GetResult();

            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(ScheduleData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            //if we call again, should not fetch again
            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.AreEqual(ScheduleData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void NumberOfSimpleTeamProviderCallsMatchIsCorrect()
        {
            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames1);

            Assert.AreEqual(ScheduleData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var competitorNames2 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames2);

            Assert.AreNotEqual(competitorNames1.Id, competitorNames2.Id);
            Assert.AreEqual(ScheduleData.Cultures3.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void SimpleTeamIsCachedWithoutBetradarId()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            var competitorNamesCI = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNamesCI);
            Assert.IsNotNull(competitorNamesCI.ReferenceId);
            Assert.IsNotNull(competitorNamesCI.ReferenceId.ReferenceIds);
            Assert.IsTrue(competitorNamesCI.ReferenceId.ReferenceIds.Any());
            Assert.AreEqual(1, competitorNamesCI.ReferenceId.ReferenceIds.Count);
            Assert.AreEqual(1, competitorNamesCI.ReferenceId.BetradarId);
        }

        [TestMethod]
        public void SimpleTeamIsCachedWithoutReferenceIds()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            var simpleTeamDto = CacheSimpleTeam(1, null);

            var competitorNamesCI = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(simpleTeamDto.Competitor.Id, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNamesCI);
            Assert.AreEqual(simpleTeamDto.Competitor.Id, competitorNamesCI.Id);
            Assert.IsNotNull(competitorNamesCI.ReferenceId);
            Assert.IsNotNull(competitorNamesCI.ReferenceId.ReferenceIds);
            Assert.IsTrue(competitorNamesCI.ReferenceId.ReferenceIds.Any());
            Assert.AreEqual(1, competitorNamesCI.ReferenceId.ReferenceIds.Count);
            Assert.AreEqual(simpleTeamDto.Competitor.Id.Id, competitorNamesCI.ReferenceId.BetradarId);
        }

        [TestMethod]
        public void SimpleTeamIsCachedWithBetradarId()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            var competitorNamesCI = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNamesCI);
            Assert.IsNotNull(competitorNamesCI.ReferenceId);
            Assert.IsNotNull(competitorNamesCI.ReferenceId.ReferenceIds);
            Assert.IsTrue(competitorNamesCI.ReferenceId.ReferenceIds.Any());
            Assert.AreEqual(2, competitorNamesCI.ReferenceId.ReferenceIds.Count);
            Assert.AreEqual("555", competitorNamesCI.ReferenceId.BetradarId.ToString());
        }

        [TestMethod]
        public void SimpleTeamCanBeRemovedFromCache()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            var simpleTeamDto = CacheSimpleTeam(654321, null);

            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            var cacheItem = _sportEntityFactoryBuilder.ProfileMemoryCache.GetCacheItem(simpleTeamDto.Competitor.Id.ToString());
            Assert.IsNotNull(cacheItem);
            Assert.AreEqual(simpleTeamDto.Competitor.Id.ToString(), cacheItem.Key);

            _sportEntityFactoryBuilder.CacheManager.RemoveCacheItem(simpleTeamDto.Competitor.Id, CacheItemType.Competitor, "Test");
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            cacheItem = _sportEntityFactoryBuilder.ProfileMemoryCache.GetCacheItem(simpleTeamDto.Competitor.Id.ToString());
            Assert.IsNull(cacheItem);
        }

        [TestMethod]
        public void CachePlayerDataFromPlayerProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.First()), playerNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, playerNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, playerNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(playerNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.First()), playerNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.Skip(1).First()), playerNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.Skip(2).First()), playerNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public void CacheCompetitorDataFromCompetitorProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures1, true).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public void CacheCompetitorDataFromMatchSummary()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureEn, null).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void CacheCompetitorDataFromTournamentSummary()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchTournamentId, ScheduleData.CultureEn, null).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public void CacheCompetitorDataFromSeasonSummary()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchSeasonId, ScheduleData.CultureEn, null).GetAwaiter().GetResult();
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
            Assert.IsNotNull(competitorNames);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        private SimpleTeamProfileDTO CacheSimpleTeam(int id, IDictionary<string, string> referenceIds)
        {
            var simpleTeam = MessageFactoryRest.GetSimpleTeamCompetitorProfileEndpoint(id, referenceIds);
            var simpleTeamDto = new SimpleTeamProfileDTO(simpleTeam);
            _sportEntityFactoryBuilder.CacheManager.SaveDto(simpleTeamDto.Competitor.Id, simpleTeamDto, CultureInfo.CurrentCulture, DtoType.SimpleTeamProfile, null);
            return simpleTeamDto;
        }
    }
}
