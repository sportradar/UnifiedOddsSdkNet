using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class BookingManagerTests
    {
        private TestDataFetcher _dataFetcher;
        private IBookingManager _bookingManager;
        private IBookingManager _bookingManagerCatch;
        private const string BookingUrl = "booking-calendar";

        [TestInitialize]
        public void Initialize()
        {
            _dataFetcher = new TestDataFetcher();
            var cacheManager = new CacheManager();
            var config1 = TestConfigurationInternal.GetConfig();
            _bookingManager = new BookingManager(config1, _dataFetcher, cacheManager);
            var config2 = TestConfigurationInternal.GetConfig();
            config2.ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            _bookingManagerCatch = new BookingManager(config2, _dataFetcher, cacheManager);
        }

        [TestMethod]
        public void BookingManagerSetupCorrectly()
        {
            Assert.IsNotNull(_bookingManager);
            Assert.IsNotNull(_bookingManagerCatch);
        }

        [TestMethod]
        public void BookLiveOddsEventSucceeded()
        {
            const string xml = "/booking-calendar/";
            _dataFetcher.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>(xml, 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            var isBooked = _bookingManager.BookLiveOddsEvent(ScheduleData.MatchId);
            Assert.IsTrue(isBooked);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BookLiveOddsEventNullEventIdThrows()
        {
            _bookingManager.BookLiveOddsEvent(null);
        }

        [TestMethod]
        public void BookLiveOddsEventCallsCorrectEndpoint()
        {
            const string xml = "/booking-calendar/";
            _dataFetcher.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>(xml, 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            Assert.AreEqual(0, _dataFetcher.CalledUrls.Count);

            _bookingManager.BookLiveOddsEvent(ScheduleData.MatchId);
            Assert.AreEqual(1, _dataFetcher.CalledUrls.Count);
            Assert.IsTrue(_dataFetcher.CalledUrls.First().Contains(BookingUrl));
        }

        [TestMethod]
        public void BookLiveOddsEventCallRespectExceptionHandlingStrategyCatch()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "/booking-calendar/";
            _dataFetcher.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            var isBooked = _bookingManagerCatch.BookLiveOddsEvent(ScheduleData.MatchId);
            Assert.IsFalse(isBooked);
            Assert.AreEqual(1, _dataFetcher.CalledUrls.Count);
            Assert.IsTrue(_dataFetcher.CalledUrls.First().Contains(BookingUrl));
        }

        [TestMethod]
        public void BookLiveOddsEventCallRespectExceptionHandlingStrategyThrow()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "/booking-calendar/";
            _dataFetcher.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            var isBooked = false;
            try
            {
                isBooked = _bookingManager.BookLiveOddsEvent(ScheduleData.MatchId);
            }
            catch (Exception e)
            {
                var exception = (CommunicationException)e;
                Assert.IsFalse(isBooked);
                Assert.IsNotNull(exception);
                Assert.AreEqual(HttpStatusCode.NotFound, exception.ResponseCode);
                Assert.AreEqual(exceptionMessage, exception.Message);
                Assert.AreEqual(xml, exception.Url);
            }
            Assert.AreEqual(1, _dataFetcher.CalledUrls.Count);
            Assert.IsTrue(_dataFetcher.CalledUrls.First().Contains(BookingUrl));
        }
    }
}
