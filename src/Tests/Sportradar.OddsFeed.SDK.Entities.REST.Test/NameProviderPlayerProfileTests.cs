/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class NameProviderPlayerProfileTests
    {
        private INameProvider _nameProvider;
        private TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;
        private ScheduleData _scheduleData;

        [TestInitialize]
        public void Init()
        {
            _scheduleData = new ScheduleData(new TestSportEntityFactoryBuilder(ScheduleData.Cultures3));
            _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(ScheduleData.Cultures3);
            var match = _sportEntityFactoryBuilder.SportEntityFactory.BuildSportEvent<IMatch>(ScheduleData.MatchId, ScheduleData.MatchSportId, ScheduleData.Cultures3, _sportEntityFactoryBuilder.ThrowingStrategy);
            _nameProvider = new NameProvider(new Mock<IMarketCacheProvider>().Object,
                                             _sportEntityFactoryBuilder.ProfileCache,
                                             new Mock<INameExpressionFactory>().Object,
                                             match,
                                             1,
                                             null,
                                             ExceptionHandlingStrategy.THROW);
        }

        [TestMethod]
        [ExpectedException(typeof(NameGenerationException))]
        public void OutcomeIdWithPlayerIdMissingIdThrows()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            _nameProvider.GetOutcomeNameAsync("sr:player2", ScheduleData.CultureEn).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(NameGenerationException))]
        public void OutcomeIdWithWrongPlayerTypeThrows()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            _nameProvider.GetOutcomeNameAsync("sr:customplayer:2", ScheduleData.CultureEn).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(NameGenerationException))]
        public void CompositeOutcomeIdWithSpaceInDelimiterThrows()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            _nameProvider.GetOutcomeNameAsync("sr:player:2, sr:competitor:1", ScheduleData.CultureEn).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(NameGenerationException))]
        public void CompositeOutcomeIdWithWrongDelimiterThrows()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            _nameProvider.GetOutcomeNameAsync("sr:player:2;sr:competitor:1", ScheduleData.CultureEn).GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task OutcomeIdWithSinglePlayerIdGetsCorrectValue()
        {
            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId1.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player1.Names[ScheduleData.CultureEn], name);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.PlayerProfileMarketPrefix)));
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompositeOutcomeIdForTwoPlayerIdsSameCompetitorGetsCorrectValue()
        {
            var outcomeId = $"{ScheduleData.MatchCompetitor1PlayerId1},{ScheduleData.MatchCompetitor1PlayerId2}";
            var expectedName = $"{_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn)}";

            var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
            Assert.AreEqual(expectedName, name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompositeOutcomeIdForTwoPlayerIdsDifferentCompetitorGetsCorrectValue()
        {
            var outcomeId = $"{ScheduleData.MatchCompetitor1PlayerId1},{ScheduleData.MatchCompetitor2PlayerId2}";
            var expectedName = $"{_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor2Player2.GetName(ScheduleData.CultureEn)}";

            var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
            Assert.AreEqual(expectedName, name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompositeOutcomeIdForTwoCompetitorIdsGetsCorrectValue()
        {
            var outcomeId = $"{ScheduleData.MatchCompetitorId1},{ScheduleData.MatchCompetitorId2}";
            var expectedName = $"{_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor2.GetName(ScheduleData.CultureEn)}";

            var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
            Assert.AreEqual(expectedName, name);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task PreloadedPlayerDataAndOutcomeIdWithSinglePlayerIdGetsCorrectValue()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetPlayerProfileAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.CultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId1.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task PreloadedCompetitorDataAndOutcomeIdWithSinglePlayerIdGetsCorrectValue()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId1, ScheduleData.CultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId2.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [TestMethod]
        public async Task WhenMissingDataPlayerProfileIsCalledOnlyOnce()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId2.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor2Player2.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId2.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor2Player2.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompositeOutcomeIdWithCompetitorAndPlayerIsCalledCorrectly()
        {
            var outcomeId = $"{ScheduleData.MatchCompetitorId1},{ScheduleData.MatchCompetitor1PlayerId2}";
            var expectedName = $"{_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn)}";
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
            Assert.AreEqual(expectedName, name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompositeOutcomeIdWithPlayerAndCompetitorIsCalledCorrectly()
        {
            var outcomeId = $"{ScheduleData.MatchCompetitor1PlayerId2},{ScheduleData.MatchCompetitorId1}";
            var expectedName = $"{_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn)}";
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
            Assert.AreEqual(expectedName, name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task WhenNoDataPlayerProfileIsCalledOnlyOncePerCulture()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1Player2.GetName(cultureInfo), name);
                Assert.IsFalse(string.IsNullOrEmpty(name));
            }
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(7, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(6, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompetitorDataIsLoadedFromMatchSummary()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name);
            }
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompetitorProfileIsCalledOnlyOnce()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompetitorProfileIsLoadedFromSummaryOnlyOncePerCulture()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name);
                Assert.IsFalse(string.IsNullOrEmpty(name));
            }
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task PreloadedMatchDataAndOutcomeIdWithSingleCompetitorIdGetsCorrectValue()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public async Task CompetitorProfileIsCalledWhenWantedPlayerProfileLinkedToCompetitor()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId2, ScheduleData.CultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(ScheduleData.MatchCompetitor2PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId1.ToString(), ScheduleData.CultureEn);
            Assert.AreEqual(_scheduleData.MatchCompetitor2Player1.GetName(ScheduleData.CultureEn), name);
            Assert.AreEqual(ScheduleData.MatchCompetitor2PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId1.ToString(), ScheduleData.CultureHu);
            Assert.AreEqual(_scheduleData.MatchCompetitor2Player1.GetName(ScheduleData.CultureHu), name);
            Assert.AreEqual(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(4, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompetitorDataIsLoadedFromTournamentSummary()
        {
            var tournament = _sportEntityFactoryBuilder.SportEntityFactory.BuildSportEvent<ITournament>(ScheduleData.MatchTournamentId, ScheduleData.MatchSportId, ScheduleData.Cultures3, _sportEntityFactoryBuilder.ThrowingStrategy);
            var nameProvider = new NameProvider(new Mock<IMarketCacheProvider>().Object,
                                                _sportEntityFactoryBuilder.ProfileCache,
                                             new Mock<INameExpressionFactory>().Object,
                                             tournament,
                                             1,
                                             null,
                                             ExceptionHandlingStrategy.THROW);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
                var name2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
            }
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task CompetitorDataIsLoadedFromSeasonSummary()
        {
            var season = _sportEntityFactoryBuilder.SportEntityFactory.BuildSportEvent<ISeason>(ScheduleData.MatchSeasonId, ScheduleData.MatchSportId, ScheduleData.Cultures3, _sportEntityFactoryBuilder.ThrowingStrategy);
            var nameProvider = new NameProvider(new Mock<IMarketCacheProvider>().Object,
                                             _sportEntityFactoryBuilder.ProfileCache,
                                             new Mock<INameExpressionFactory>().Object,
                                             season,
                                             1,
                                             null,
                                             ExceptionHandlingStrategy.THROW);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
                var name2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
            }
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task MatchDataOverridesCompetitorAssociatedEventIdOverTournamentData()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchTournamentId, ScheduleData.CultureEn, null).ConfigureAwait(false);
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null).ConfigureAwait(false);
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
                var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
            }
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("tournament")));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task TournamentDataDoesNotOverridesCompetitorAssociatedEventIdOverMatchData()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null).ConfigureAwait(false);
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchTournamentId, ScheduleData.CultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
                var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
            }
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("tournament")));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task MatchDataOverridesCompetitorAssociatedEventIdOverSeasonData()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchSeasonId, ScheduleData.CultureEn, null).ConfigureAwait(false);
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null).ConfigureAwait(false);
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
                var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
            }
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("season")));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [TestMethod]
        public async Task SeasonDataDoesNotOverridesCompetitorAssociatedEventIdOverMatchData()
        {
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null).ConfigureAwait(false);
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchSeasonId, ScheduleData.CultureEn, null).ConfigureAwait(false);
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            foreach (var cultureInfo in ScheduleData.Cultures3)
            {
                var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
                var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
                Assert.AreEqual(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
            }
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("season")));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }
    }
}
