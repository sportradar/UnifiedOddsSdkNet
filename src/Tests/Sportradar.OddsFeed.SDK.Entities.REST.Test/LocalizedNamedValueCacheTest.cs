/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class LocalizedNamedValueCacheTest
    {
        private Mock<IDataFetcher> _fetcherMock;
        private Uri _enMatchStatusUri;
        private Uri _deMatchStatusUri;
        private Uri _huMatchStatusUri;
        private Uri _nlMatchStatusUri;

        private LocalizedNamedValueCache Setup(ExceptionHandlingStrategy exceptionStrategy)
        {
            var dataFetcher = new TestDataFetcher();
            _fetcherMock = new Mock<IDataFetcher>();

            _enMatchStatusUri = new Uri(TestData.RestXmlPath + "/match_status_descriptions_en.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_enMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_enMatchStatusUri));

            _deMatchStatusUri = new Uri(TestData.RestXmlPath + "/match_status_descriptions_de.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_deMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_deMatchStatusUri));

            _huMatchStatusUri = new Uri(TestData.RestXmlPath + "/match_status_descriptions_hu.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_huMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_huMatchStatusUri));

            _nlMatchStatusUri = new Uri(TestData.RestXmlPath + "/match_status_descriptions_nl.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_nlMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_nlMatchStatusUri));

            var uriFormat = TestData.RestXmlPath + "/match_status_descriptions_{0}.xml";
            return new LocalizedNamedValueCache(new NamedValueDataProvider(uriFormat, _fetcherMock.Object, "match_status"),
                new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu") }, exceptionStrategy, "MatchStatus");
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "Allowed in this test")]
        public async Task DataIsFetchedOnlyOncePerLocale()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            var namedValue = await cache.GetAsync(0);
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("en") });
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("de") });
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("hu") });

            Assert.IsNotNull(namedValue);

            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);

            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("nl") });
            namedValue = await cache.GetAsync(0, TestData.Cultures4);

            Assert.IsNotNull(namedValue);

            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Once);
        }

        [TestMethod]
        public void InitialDataFetchDoesNotBlockConstructor()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Never);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Never);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Never);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);
        }

        [TestMethod]
        public void InitialDataFetchStartedByConstructor()
        {
            Setup(ExceptionHandlingStrategy.CATCH);

            var finished = ExecutionHelper.WaitToComplete(() =>
            {
                _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
                _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
                _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
                _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);
            }, 5000);
            Assert.IsTrue(finished);
        }

        [TestMethod]
        public async Task CorrectValuesAreLoaded()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            var doc = XDocument.Load($"{TestData.RestXmlPath}/match_status_descriptions_en.xml");
            Assert.IsNotNull(doc);
            Assert.IsNotNull(doc.Element("match_status_descriptions"));

            foreach (var xElement in doc.Element("match_status_descriptions").Elements("match_status"))
            {
                Assert.IsNotNull(xElement.Attribute("id"));
                var id = int.Parse(xElement.Attribute("id").Value);
                var namedValue = await cache.GetAsync(id);

                Assert.IsNotNull(namedValue);
                Assert.AreEqual(id, namedValue.Id);

                Assert.IsTrue(namedValue.Descriptions.ContainsKey(new CultureInfo("en")));
                Assert.IsTrue(namedValue.Descriptions.ContainsKey(new CultureInfo("de")));
                Assert.IsTrue(namedValue.Descriptions.ContainsKey(new CultureInfo("hu")));
                Assert.IsFalse(namedValue.Descriptions.ContainsKey(new CultureInfo("nl")));

                Assert.AreNotEqual(namedValue.GetDescription(new CultureInfo("en")), new CultureInfo("de").Name);
                Assert.AreNotEqual(namedValue.GetDescription(new CultureInfo("en")), new CultureInfo("hu").Name);
                Assert.AreNotEqual(namedValue.GetDescription(new CultureInfo("de")), new CultureInfo("hu").Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task ThrowingExceptionStrategyIsRespected()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            await cache.GetAsync(1000);
        }

        [TestMethod]
        public async Task CatchingExceptionStrategyIsRespected()
        {
            var cache = Setup(ExceptionHandlingStrategy.CATCH);
            var value = await cache.GetAsync(1000);

            Assert.AreEqual(1000, value.Id);
            Assert.IsNotNull(value.Descriptions);
            Assert.IsFalse(value.Descriptions.Any());
        }
    }
}
