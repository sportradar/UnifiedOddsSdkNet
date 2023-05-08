/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class LogHttpDataFetcherTest
    {
        private LogHttpDataFetcher _logHttpDataFetcher;
        private LogHttpDataFetcher _logHttpDataFetcherPool;

        [TestInitialize]
        public void Init()
        {
            var httpMessageHandler = new TestMessageHandler(200, 0);
            var httpClient = new HttpClient(httpMessageHandler);
            var sdkHttpClient = new SdkHttpClient("aaa", httpClient);
            var sdkHttpClientPool = new SdkHttpClientPool("aaa", 200, TimeSpan.FromSeconds(5), httpMessageHandler);

            _logHttpDataFetcher = new LogHttpDataFetcher(sdkHttpClient, new IncrementalSequenceGenerator(), new Deserializer<response>());
            _logHttpDataFetcherPool = new LogHttpDataFetcher(sdkHttpClientPool, new IncrementalSequenceGenerator(), new Deserializer<response>());
        }

        //[TestMethod]
        [Timeout(30000)]
        public async Task PerformanceOf100SequentialRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
            {
                var result = await _logHttpDataFetcher.GetDataAsync(GetRequestUri(false)).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.CanRead);
            }
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task PerformanceOfManyParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var task = _logHttpDataFetcher.GetDataAsync(GetRequestUri(false));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.IsTrue(tasks.TrueForAll(a => a.IsCompleted));
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task PerformancePoolOfManyParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var task = _logHttpDataFetcherPool.GetDataAsync(GetRequestUri(false));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.IsTrue(tasks.TrueForAll(a => a.IsCompleted));
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task PerformanceOfManyUniqueParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var task = _logHttpDataFetcher.GetDataAsync(GetRequestUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.IsTrue(tasks.TrueForAll(a => a.IsCompleted));
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task PerformancePoolOfManyUniqueParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var task = _logHttpDataFetcherPool.GetDataAsync(GetRequestUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.IsTrue(tasks.TrueForAll(a => a.IsCompleted));
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task PerformanceOfManyUniqueUriParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var task = _logHttpDataFetcher.GetDataAsync(GetRandomUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.IsTrue(tasks.TrueForAll(a => a.IsCompleted));
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task PerformancePoolOfManyUniqueUriParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var task = _logHttpDataFetcherPool.GetDataAsync(GetRandomUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.IsTrue(tasks.TrueForAll(a => a.IsCompleted));
            Debug.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        private Uri GetRequestUri(bool isRandom)
        {
            var id = isRandom ? SdkInfo.GetRandom() : 1;
            return new Uri($"http://test.domain.com/api/v1/sr:match:{id}/summary.xml");
        }

        private Uri GetRandomUri(bool isRandom)
        {
            var id = isRandom ? SdkInfo.GetRandom() : 1;
            return new Uri($"http://test.domain.com/api/v1/sr:match:{id}.xml");
        }

        private class TestMessageHandler : HttpMessageHandler
        {
            private readonly int _timeoutMs;
            private readonly int _timeoutVariablePercent;

            public TestMessageHandler(int timeoutMs, int timeoutVariablePercent = 0)
            {
                _timeoutMs = timeoutMs;
                _timeoutVariablePercent = timeoutVariablePercent;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var stopWatch = Stopwatch.StartNew();
                var timeout = _timeoutVariablePercent < 1 ? _timeoutMs : SdkInfo.GetVariableNumber(_timeoutMs, _timeoutVariablePercent);

                await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);
                Debug.WriteLine($"Request to {request.RequestUri} took {stopWatch.ElapsedMilliseconds} ms");
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    ReasonPhrase = HttpStatusCode.Accepted.ToString(),
                    RequestMessage = request,
                    Content = new StringContent("some text")
                };
            }
        }
    }
}
