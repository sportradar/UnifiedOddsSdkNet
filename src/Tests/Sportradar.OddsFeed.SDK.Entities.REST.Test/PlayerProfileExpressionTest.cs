/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class PlayerProfileExpressionTest
    {
        private readonly IOperandFactory _operandFactory = new OperandFactory();
        private TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;

        [TestInitialize]
        public void Init()
        {
            _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(ScheduleData.Cultures3.ToList());
        }

        [TestMethod]
        public async Task NotCachedPlayerProfileExpressionReturnsCorrectValue()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));

            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task NotCachedPlayerProfileMultiCallExpressionInvokesEndpointOnlyOnce()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));

            var result1 = await expression.BuildNameAsync(TestData.Culture);
            var result2 = await expression.BuildNameAsync(TestData.Culture);
            var result3 = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result3, result2);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result1);
        }

        [TestMethod]
        public async Task NotCachedCompetitorProfileExpressionReturnsCorrectValue()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains("competitor")));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task NotCachedCompetitorProfileMultiCallExpressionInvokesEndpointOnlyOnce()
        {
            Assert.AreEqual(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

            var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

            var result1 = await expression.BuildNameAsync(TestData.Culture);
            var result2 = await expression.BuildNameAsync(TestData.Culture);
            var result3 = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result3, result2);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains("competitor")));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result1);
        }

        [TestMethod]
        public async Task CachedPlayerProfileExpressionDoesNotInvokeAgain()
        {
            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task CachedFromCompetitorPlayerProfileExpressionDoesNotInvokeAgain()
        {
            await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.GetCount());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task CachedCompetitorProfileExpressionDoesNotInvokeAgain()
        {
            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains("competitor")));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains("competitor")));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task CachedFromMatchCompetitorProfileExpressionDoesNotInvokeAgain()
        {
            var matchId = URN.Parse("sr:match:1");
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, TestData.Culture, null);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

            _ = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public async Task CachedFromStageCompetitorProfileExpressionDoesNotInvokeAgain()
        {
            const string competitorId = "sr:competitor:7135";
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(TestData.EventStageId, TestData.Culture, null);
            Assert.AreEqual(22, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

            _ = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(22, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public async Task CachedFromSeasonCompetitorProfileExpressionDoesNotInvokeAgain()
        {
            const string competitorId = "sr:competitor:2";
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(TestData.SeasonId, TestData.Culture, null);
            Assert.AreEqual(20, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(20, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse(competitorId), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task CachedFromTournamentCompetitorProfileExpressionDoesNotInvokeAgain()
        {
            const string tournamentId = "sr:tournament:1030";
            const string competitorId = "sr:competitor:66412";
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(URN.Parse(tournamentId), TestData.Culture, null);
            Assert.AreEqual(16, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

            _ = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(16, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [TestMethod]
        public async Task CachedFromScheduleForDateCompetitorProfileExpressionDoesNotInvokeAgain()
        {
            const string competitorId = "sr:competitor:66390";
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventsForDateAsync(DateTime.Now, TestData.Culture);
            Assert.AreEqual(1796, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventsForDate));

            var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
            var result = await expression.BuildNameAsync(TestData.Culture);
            Assert.AreEqual(1796, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventsForDate));

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse(competitorId), new[] { TestData.Culture }, true);
            Assert.AreEqual(profile.GetName(TestData.Culture), result);
        }

        [TestMethod]
        public async Task PartiallyPlayerProfileExpressionDoesInvokeForMissingCultures()
        {
            var cultures = TestData.Cultures3.ToList();

            _ = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { cultures[0] }, true);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
            var result1 = await expression.BuildNameAsync(cultures[0]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var result2 = await expression.BuildNameAsync(cultures[1]);
            var result3 = await expression.BuildNameAsync(cultures[2]);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result1, result3);

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { cultures[0] }, true);
            Assert.AreEqual(profile.GetName(cultures[0]), result1);
        }

        [TestMethod]
        public async Task PartiallyCachedFromCompetitorPlayerProfileExpressionDoesInvokeForMissingCultures()
        {
            var cultures = TestData.Cultures3.ToList();

            _ = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), new[] { cultures[0] }, true);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
            var result1 = await expression.BuildNameAsync(cultures[0]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.GetCount());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var result2 = await expression.BuildNameAsync(cultures[1]);
            var result3 = await expression.BuildNameAsync(cultures[2]);
            Assert.AreEqual(35, _sportEntityFactoryBuilder.ProfileMemoryCache.GetCount());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile)); // since player is associated with competitor, that will be invoked instead of player profile
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result1, result3);

            var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { cultures[0] }, true);
            Assert.AreEqual(profile.GetName(cultures[0]), result1);
        }

        [TestMethod]
        public async Task PartiallyCachedFromMatchCompetitorProfileExpressionDoesInvokeForMissingCultures()
        {
            var cultures = TestData.Cultures3.ToList();
            var matchId = URN.Parse("sr:match:1");
            await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[0], null);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
            var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
            await expression.BuildNameAsync(cultures[0]);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            await expression.BuildNameAsync(cultures[1]);
            await expression.BuildNameAsync(cultures[2]);
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count(c => c.Key.Contains("competitor")));
            Assert.AreEqual(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetCount());
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.AreEqual(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.AreEqual(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }
    }
}
