using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;
using System.Runtime.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using RMF=Sportradar.OddsFeed.SDK.Test.Shared.RestMessageEntityFactory;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class CacheMessageProcessorTest
    {
        private static readonly IDeserializer<FeedMessage> Deserializer = new Deserializer<FeedMessage>();
        private static readonly string DirPath = Directory.GetCurrentDirectory() + @"\XMLs";
        private const string InputXml = "odds_change.xml";
        private const string CachePrefix = "EventStatus_";

        private const string FixtureXml = "fixtures.{1}.xml";
        private const string ScheduleXml = "schedule.{1}.xml";
        private const string TourXml = "tournament_schedule.{1}.xml";
        private const string DetailsXml = "event_details.{1}.xml";

        private MemoryCache _statusCache;
        private MemoryCache _eventCache;
        private CacheMessageProcessor _processor;

        private odds_change _record;

        private SportEventCache _sportEventCache;
        private SportEventCacheItemFactory _sportEventCacheItemFactory;

        private static readonly CultureInfo DefaultCulture = new CultureInfo("en");
        private static readonly List<CultureInfo> Cultures = new List<CultureInfo>(new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu") });

        [TestInitialize]
        public void Init()
        {
            InitSportEventCache();
            _statusCache = new MemoryCache("eventsStatus");
            _processor = new CacheMessageProcessor(_statusCache, new SportEventStatusMapperFactory(), _sportEventCache);

            var data = FileHelper.OpenFile(DirPath, InputXml);
            _record = Deserializer.Deserialize<odds_change>(data);
            _record.SportId = URN.Parse("sr:sport:1000");
        }

        private void InitSportEventCache()
        {
            var testDataFetcher = new TestDataFetcher();
            _eventCache = new MemoryCache("test");
            var fixtureDeserializer = new Deserializer<fixturesEndpoint>();
            var fixtureMapperFactory = new FixtureMapperFactory();

            var fixtureProvider = new DataProvider<fixturesEndpoint, FixtureDTO>(
                DirPath + FixtureXml,
                testDataFetcher, 
                fixtureDeserializer,
                fixtureMapperFactory);

            var dateDeserializer = new Deserializer<scheduleEndpoint>();
            var dateMapperFactory = new SportEventsScheduleMapperFactory();

            var dateProvider = new DateScheduleProvider(
                DirPath + ScheduleXml,
                DirPath + ScheduleXml,
                testDataFetcher,
                dateDeserializer,
                dateMapperFactory);

            // tournament schedule provider
            var tourDeserializer = new Deserializer<tournamentScheduleEndpoint>();
            var tourMapperFactory = new TournamentScheduleMapperFactory();

            var tourProvider = new DataProvider<tournamentScheduleEndpoint, EntityList<MatchSummaryDTO>>(
                DirPath + TourXml,
                testDataFetcher,
                tourDeserializer,
                tourMapperFactory);
            
            var detailsDeserializer = new Deserializer<matchSummaryEndpoint>();
            var detailsMapperFactory = new SportEventDetailsMapperFactory();

            var eventDetailsProvider = new DataProvider<matchSummaryEndpoint, SportEventDetailsDTO>(
                DirPath + DetailsXml,
                testDataFetcher,
                detailsDeserializer,
                detailsMapperFactory);

            var timer = new TestTimer(false);

            _sportEventCacheItemFactory = new SportEventCacheItemFactory(fixtureProvider, eventDetailsProvider, DefaultCulture);

            _sportEventCache = new SportEventCache(_eventCache, dateProvider, tourProvider, _sportEventCacheItemFactory, timer, Cultures);
        }

        [TestMethod]
        public void ProcessOddChangeMsg()
        {
            _processor.ProcessMessage(new alive() { product = (int)Product.LCOO });
            Assert.AreEqual(0, _statusCache.GetCount(), "Cache must be empty.");

            _processor.ProcessMessage(new odds_change() { product = (int)Product.LCOO, request_idSpecified = true, request_id = 1 });
            _processor.ProcessMessage(new bet_settlement() { product = (int)Product.LCOO, request_idSpecified = true, request_id = 1 });
            Assert.AreEqual(0, _statusCache.GetCount(), "Cache must be empty.");

            _processor.ProcessMessage(_record);
            Assert.AreEqual(1, _statusCache.GetCount(), "Cache must have 1 record.");
            var status = (ISportEventStatus) _statusCache.Get(CachePrefix + _record.event_id);
            Assert.IsNotNull(status, "ISportEventStatus cant be null.");
            Assert.AreEqual(status.AwayScore, status.AwayScore, "AwayScore not equal.");
            Assert.AreEqual(status.HomeScore, status.HomeScore, "HomeScore not equal.");
            Assert.AreEqual(status.IsReported, status.IsReported, "IsReported not equal.");
            Assert.AreEqual(status.Status, status.Status, "Status not equal.");

            foreach (var s in status.Properties)
            {
                object o = status.GetPropertyValue(s.Key);
                Assert.AreEqual(s.Value, o, $"Value of key {s.Key} not equal.");
            }

            _processor.ProcessMessage(_record);
            _processor.ProcessMessage(_record);
            _record.event_id = "sr:match:10000";
            _processor.ProcessMessage(_record);
            Assert.AreEqual(2, _statusCache.GetCount(), "Cache must have 2 records.");
        }

        [TestMethod]
        public void CheckProcessingOddsChangeAndRetrievingStatus()
        {
            _processor.ProcessMessage(_record);
            var status = ((ISportEventStatusCache)_processor).GetSportEventStatus(URN.Parse(_record.EventId));

            Assert.IsNotNull(status, "ISportEventStatus cant be null.");
            Assert.AreEqual(status.AwayScore, status.AwayScore, "AwayScore not equal.");
            Assert.AreEqual(status.HomeScore, status.HomeScore, "HomeScore not equal.");
            Assert.AreEqual(status.IsReported, status.IsReported, "IsReported not equal.");
            Assert.AreEqual(status.Status, status.Status, "Status not equal.");

            foreach (var s in status.Properties)
            {
                object o = status.GetPropertyValue(s.Key);
                Assert.AreEqual(s.Value, o, $"Value of key {s.Key} not equal.");
            }
        }

        [TestMethod]
        public void PurgeSportEventCacheItem()
        {
            Assert.AreEqual(0, _eventCache.Count());

            var ci = _sportEventCacheItemFactory.Build(new FixtureDTO(RMF.GetFixture()), DefaultCulture);
            _eventCache.Add(new CacheItem(ci.Id.ToString(), ci), null);
            Assert.AreEqual(1, _eventCache.Count());

            _sportEventCache.PurgeItem(ci.Id);

            Assert.AreEqual(0, _eventCache.Count());
        }
    }
}
