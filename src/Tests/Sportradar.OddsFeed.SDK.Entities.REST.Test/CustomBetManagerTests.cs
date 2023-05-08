using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class CustomBetManagerTests
    {
        private TestDataRouterManager _dataRouterManager;
        private ICustomBetManager _customBetManager;
        private ICustomBetManager _customBetManagerCatch;

        [TestInitialize]
        public void Initialize()
        {
            _dataRouterManager = new TestDataRouterManager(new CacheManager());
            var customBetSelectionBuilder = new CustomBetSelectionBuilder();
            _customBetManager = new CustomBetManager(_dataRouterManager, customBetSelectionBuilder, ExceptionHandlingStrategy.THROW);
            _customBetManagerCatch = new CustomBetManager(_dataRouterManager, customBetSelectionBuilder, ExceptionHandlingStrategy.CATCH);
        }

        [TestMethod]
        public void CustomBetManagerSetupCorrectly()
        {
            Assert.IsNotNull(_customBetManager);
            Assert.IsNotNull(_customBetManagerCatch);
        }

        [TestMethod]
        public async Task AvailableSelectionReturnsMarkets()
        {
            var availableSelections = await _customBetManager.GetAvailableSelectionsAsync(ScheduleData.MatchId);
            Assert.IsNotNull(availableSelections);
            Assert.AreEqual(ScheduleData.MatchId, availableSelections.Event);
            Assert.IsNotNull(availableSelections.Markets);
            Assert.IsFalse(availableSelections.Markets.IsNullOrEmpty());
        }

        [TestMethod]
        public async Task AvailableSelectionCallsCorrectEndpoint()
        {
            Assert.AreEqual(0, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(0, _dataRouterManager.RestUrlCalls.Count);

            await _customBetManager.GetAvailableSelectionsAsync(ScheduleData.MatchId);

            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointAvailableSelections, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task AvailableSelectionCallRespectExceptionHandlingStrategyCatch()
        {
            const string xml = "available_selections.xml";
            _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException("Not found for id", xml, HttpStatusCode.NotFound, null)));

            var availableSelections = await _customBetManagerCatch.GetAvailableSelectionsAsync(ScheduleData.MatchId);
            Assert.IsNull(availableSelections);
            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointAvailableSelections, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task AvailableSelectionCallRespectExceptionHandlingStrategyThrow()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "available_selections.xml";
            _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;
            _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            IAvailableSelections availableSelections = null;
            try
            {
                availableSelections = await _customBetManager.GetAvailableSelectionsAsync(ScheduleData.MatchId);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
                Assert.IsInstanceOfType(e, typeof(CommunicationException));
                var exception = (CommunicationException)e;
                Assert.IsNotNull(exception);
                Assert.AreEqual(HttpStatusCode.NotFound, exception.ResponseCode);
                Assert.AreEqual(exceptionMessage, exception.Message);
                Assert.AreEqual(xml, exception.Url);
            }
            Assert.IsNull(availableSelections);
            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointAvailableSelections, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task CalculateProbabilityReturnsCalculation()
        {
            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            var calculation = await _customBetManager.CalculateProbabilityAsync(selections);
            Assert.IsNotNull(calculation);
            Assert.IsTrue(calculation.Odds > 0);
            Assert.IsTrue(calculation.Probability > 0);
            Assert.IsNotNull(calculation.AvailableSelections);
            Assert.IsFalse(calculation.AvailableSelections.IsNullOrEmpty());
        }

        [TestMethod]
        public async Task CalculateProbabilityCallsCorrectEndpoint()
        {
            Assert.AreEqual(0, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(0, _dataRouterManager.RestUrlCalls.Count);

            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            await _customBetManager.CalculateProbabilityAsync(selections);

            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointCalculateProbability, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task CalculateProbabilityCallRespectExceptionHandlingStrategyCatch()
        {
            const string xml = "calculate_response.xml";
            _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException("Not found for id", xml, HttpStatusCode.NotFound, null)));

            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            var calculation = await _customBetManagerCatch.CalculateProbabilityAsync(selections);
            Assert.IsNull(calculation);
            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointCalculateProbability, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task CalculateProbabilityCallRespectExceptionHandlingStrategyThrow()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "calculate_response.xml";
            _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;
            _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            ICalculation calculation = null;
            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            try
            {
                calculation = await _customBetManager.CalculateProbabilityAsync(selections);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
                Assert.IsInstanceOfType(e, typeof(CommunicationException));
                var exception = (CommunicationException)e;
                Assert.IsNotNull(exception);
                Assert.AreEqual(HttpStatusCode.NotFound, exception.ResponseCode);
                Assert.AreEqual(exceptionMessage, exception.Message);
                Assert.AreEqual(xml, exception.Url);
            }
            Assert.IsNull(calculation);
            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointCalculateProbability, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task CalculateProbabilityFilterReturnsCalculation()
        {
            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            var calculation = await _customBetManager.CalculateProbabilityFilterAsync(selections);
            Assert.IsNotNull(calculation);
            Assert.IsTrue(calculation.Odds > 0);
            Assert.IsTrue(calculation.Probability > 0);
            Assert.IsNotNull(calculation.AvailableSelections);
            Assert.IsFalse(calculation.AvailableSelections.IsNullOrEmpty());
        }

        [TestMethod]
        public async Task CalculateProbabilityFilterCallsCorrectEndpoint()
        {
            Assert.AreEqual(0, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(0, _dataRouterManager.RestUrlCalls.Count);

            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            await _customBetManager.CalculateProbabilityFilterAsync(selections);

            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointCalculateProbabilityFiltered, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task CalculateProbabilityFilterCallRespectExceptionHandlingStrategyCatch()
        {
            const string xml = "calculate_filter_response.xml";
            _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException("Not found for id", xml, HttpStatusCode.NotFound, null)));

            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            var calculation = await _customBetManagerCatch.CalculateProbabilityFilterAsync(selections);
            Assert.IsNull(calculation);
            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointCalculateProbabilityFiltered, _dataRouterManager.RestMethodCalls.First().Key);
        }

        [TestMethod]
        public async Task CalculateProbabilityFilterCallRespectExceptionHandlingStrategyThrow()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "calculate_filter_response.xml";
            _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;
            _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            ICalculationFilter calculation = null;
            var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
            try
            {
                calculation = await _customBetManager.CalculateProbabilityFilterAsync(selections);
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
                Assert.IsInstanceOfType(e, typeof(CommunicationException));
                var exception = (CommunicationException)e;
                Assert.IsNotNull(exception);
                Assert.AreEqual(HttpStatusCode.NotFound, exception.ResponseCode);
                Assert.AreEqual(exceptionMessage, exception.Message);
                Assert.AreEqual(xml, exception.Url);
            }
            Assert.IsNull(calculation);
            Assert.AreEqual(1, _dataRouterManager.RestMethodCalls.Count);
            Assert.AreEqual(TestDataRouterManager.EndpointCalculateProbabilityFiltered, _dataRouterManager.RestMethodCalls.First().Key);
        }
    }
}
