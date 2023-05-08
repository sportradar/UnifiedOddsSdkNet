﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Dawn;
using Metrics;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A implementation of <see cref="IDataFetcher"/> and <see cref="IDataPoster"/> which uses the HTTP requests to fetch or post the requested data. All request are logged.
    /// </summary>
    /// <seealso cref="IDataFetcher" />
    /// <remarks>ALL, DEBUG, INFO, WARN, ERROR, FATAL, OFF - the levels are defined in order of increasing priority</remarks>
    internal class LogHttpDataFetcher : HttpDataFetcher, IHealthStatusProvider
    {
        private static readonly ILog RestLog = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(LogHttpDataFetcher));

        private readonly ISequenceGenerator _sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHttpDataFetcher"/> class.
        /// </summary>
        /// <param name="client">A <see cref="ISdkHttpClient"/> used to invoke HTTP requests</param>
        /// <param name="sequenceGenerator">A <see cref="ISequenceGenerator"/> used to identify requests</param>
        /// <param name="responseDeserializer">The deserializer for unexpected response</param>
        /// <param name="connectionFailureLimit">Indicates the limit of consecutive request failures, after which it goes in "blocking mode"</param>
        /// <param name="connectionFailureTimeout">indicates the timeout after which comes out of "blocking mode" (in seconds)</param>
        public LogHttpDataFetcher(ISdkHttpClient client, ISequenceGenerator sequenceGenerator, IDeserializer<response> responseDeserializer, int connectionFailureLimit = 5, int connectionFailureTimeout = 15)
            : base(client, responseDeserializer, connectionFailureLimit, connectionFailureTimeout)
        {
            Guard.Argument(sequenceGenerator, nameof(sequenceGenerator)).NotNull();
            Guard.Argument(connectionFailureLimit, nameof(connectionFailureLimit)).Positive();
            Guard.Argument(connectionFailureTimeout, nameof(connectionFailureTimeout)).Positive();

            _sequenceGenerator = sequenceGenerator;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Stream" /> containing data fetched from the provided <see cref="Uri" />
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be fetched</param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="Stream" /> containing fetched data</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        public override async Task<Stream> GetDataAsync(Uri uri)
        {
            Metric.Context("FEED").Meter("LogHttpDataFetcher->GetDataAsync", Unit.Requests).Mark();

            var dataId = _sequenceGenerator.GetNext().ToString("D7"); // because request can take long time, there may be several request at the same time; Id to know what belongs together.

            var logBuilder = new StringBuilder();
            logBuilder.Append("Id:").Append(dataId).Append(" Fetching url: ").Append(uri.AbsoluteUri);

            var watch = Stopwatch.StartNew();
            Stream responseStream;
            try
            {
                responseStream = await base.GetDataAsync(uri).ConfigureAwait(false);
                watch.Stop();
            }
            catch (Exception ex)
            {
                watch.Stop();
                if (ex.GetType() == typeof(CommunicationException))
                {
                    var commException = (CommunicationException)ex;
                    logBuilder.Append(" ResponseCode:").Append(commException.ResponseCode);
                    logBuilder.Append(" Duration: ").Append(watch.ElapsedMilliseconds);
                    logBuilder.Append(" ms Response:").Append(commException.Response?.Replace("\n", string.Empty));
                    RestLog.Error(logBuilder);
                }
                throw;
            }

            logBuilder.Append(" Duration: ").Append(watch.ElapsedMilliseconds).Append(" ms");
            if (!RestLog.IsDebugEnabled)
            {
                RestLog.Info(logBuilder);
                return responseStream;
            }

            var responseContent = new StreamReader(responseStream).ReadToEndAsync().GetAwaiter().GetResult();
            responseContent = responseContent.Replace("\n", string.Empty);
            logBuilder.Append(" Response:").Append(responseContent);
            RestLog.Debug(logBuilder);

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            await writer.WriteAsync(responseContent);
            await writer.FlushAsync();
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Gets a <see cref="Stream" /> containing data fetched from the provided <see cref="Uri" />
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be fetched</param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="Stream" /> containing fetched data</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        public override Stream GetData(Uri uri)
        {
            Metric.Context("FEED").Meter("LogHttpDataFetcher->GetData", Unit.Requests).Mark();

            var dataId = _sequenceGenerator.GetNext().ToString("D7"); // because request can take long time, there may be several request at the same time; Id to know what belongs together

            var logBuilder = new StringBuilder();
            logBuilder.Append("Id:").Append(dataId).Append(" Fetching url: ").Append(uri.AbsoluteUri);

            var watch = Stopwatch.StartNew();
            Stream responseStream;
            try
            {
                responseStream = base.GetData(uri);
                watch.Stop();
            }
            catch (Exception ex)
            {
                watch.Stop();
                if (ex.GetType() == typeof(CommunicationException))
                {
                    var commException = (CommunicationException)ex;
                    logBuilder.Append(" ResponseCode:").Append(commException.ResponseCode);
                    logBuilder.Append(" Duration: ").Append(watch.ElapsedMilliseconds);
                    logBuilder.Append(" ms Response:").Append(commException.Response?.Replace("\n", string.Empty));
                    RestLog.Error(logBuilder);
                }
                throw;
            }

            logBuilder.Append(" Duration: ").Append(watch.ElapsedMilliseconds).Append(" ms");
            if (!RestLog.IsDebugEnabled)
            {
                RestLog.Info(logBuilder);
                return responseStream;
            }

            var responseContent = new StreamReader(responseStream).ReadToEndAsync().GetAwaiter().GetResult();
            responseContent = responseContent.Replace("\n", string.Empty);
            logBuilder.Append(" Response:").Append(responseContent);
            RestLog.Debug(logBuilder);

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(responseContent);
            writer.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Stream"/> containing data fetched from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be fetched</param>
        /// <param name="content">A <see cref="HttpContent"/> to be posted to the specific <see cref="Uri"/></param>
        /// <returns>A <see cref="Task"/> which, when successfully completed will return a <see cref="HttpResponseMessage"/></returns>
        /// <exception cref="CommunicationException">Failed to execute http post</exception>
        public override async Task<HttpResponseMessage> PostDataAsync(Uri uri, HttpContent content = null)
        {
            Metric.Context("FEED").Meter("LogHttpDataFetcher->PostDataAsync", Unit.Requests).Mark();

            var dataId = _sequenceGenerator.GetNext().ToString("D7");

            if (content != null)
            {
                try
                {
                    if (RestLog.IsDebugEnabled)
                    {
                        var s = await content.ReadAsStringAsync().ConfigureAwait(false);
                        RestLog.Debug($"Id:{dataId} Posting url: {uri.AbsoluteUri} {s}");
                    }
                    else
                    {
                        RestLog.Info($"Id:{dataId} Posting url: {uri.AbsoluteUri}");
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                RestLog.Info($"Id:{dataId} Posting url: {uri.AbsoluteUri}");
            }

            var watch = Stopwatch.StartNew();

            HttpResponseMessage response;
            try
            {
                response = await base.PostDataAsync(uri, content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                watch.Stop();
                RestLog.Error($"Id:{dataId} Posting error to {uri.AbsoluteUri} at {watch.ElapsedMilliseconds} ms.");
                if (ex.GetType() != typeof(ObjectDisposedException) && ex.GetType() != typeof(TaskCanceledException))
                {
                    RestLog.Error(ex.Message, ex);
                }
                throw;
            }

            watch.Stop();
            var responseContent = string.Empty;
            if (response.Content != null)
            {
                try
                {
                    responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }

            if (RestLog.IsDebugEnabled || !response.IsSuccessStatusCode)
            {
                var msg = $"Id:{dataId} Posting took {watch.ElapsedMilliseconds} ms. Response: {(int)response.StatusCode}-{response.ReasonPhrase} {responseContent}";
                if (!response.IsSuccessStatusCode)
                {
                    RestLog.Warn(msg);
                }
                else
                {
                    var logLevel = SdkLoggerFactory.GetWriteLogLevel(RestLog, LogLevel.Debug);
                    if (logLevel == LogLevel.Debug)
                    {
                        RestLog.Debug(msg);
                    }
                    else
                    {
                        RestLog.Info(msg);
                    }
                }
            }
            else
            {
                if (watch.ElapsedMilliseconds > 100)
                {
                    RestLog.Info($"Id:{dataId} Posting took {watch.ElapsedMilliseconds} ms.");
                }
            }
            return response;
        }

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            HealthChecks.RegisterHealthCheck("LogHttpDataFetcher", StartHealthCheck);
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        public HealthCheckResult StartHealthCheck()
        {
            return HealthCheckResult.Healthy("LogHttpDataFetcher is operational.");
        }
    }
}
