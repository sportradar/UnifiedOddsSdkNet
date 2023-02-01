﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.API.Internal.Replay
{
    /// <summary>
    /// A <see cref="IDataRestful"/> which uses the HTTP requests to post/get/put/patch and delete the data
    /// </summary>
    [Log(LoggerType.RestTraffic)]
    internal class HttpDataRestful : HttpDataFetcher, IDataRestful
    {
        private static readonly ILog Log = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(HttpDataRestful));

        /// <summary>
        /// A <see cref="ISdkHttpClient"/> used to invoke HTTP requests
        /// </summary>
        private readonly ISdkHttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDataRestful"/> class.
        /// </summary>
        /// <param name="client">A <see cref="ISdkHttpClient" /> used to invoke HTTP requests</param>
        /// <param name="responseDeserializer">The deserializer for unexpected response</param>
        /// <param name="connectionFailureLimit">Indicates the limit of consecutive request failures, after which it goes in "blocking mode"</param>
        /// <param name="connectionFailureTimeout">indicates the timeout after which comes out of "blocking mode" (in seconds)</param>
        public HttpDataRestful(ISdkHttpClient client, IDeserializer<response> responseDeserializer, int connectionFailureLimit = 5, int connectionFailureTimeout = 15)
            : base(client, responseDeserializer, connectionFailureLimit, connectionFailureTimeout)
        {
            Guard.Argument(client, nameof(client)).NotNull();
            Guard.Argument(client.DefaultRequestHeaders, nameof(client.DefaultRequestHeaders)).NotNull();
            Guard.Argument(connectionFailureLimit, nameof(connectionFailureLimit)).Positive();
            Guard.Argument(connectionFailureTimeout, nameof(connectionFailureTimeout)).Positive();

            _client = client;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of PUT request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <param name="content">A <see cref="HttpContent"/> to be posted to the specific <see cref="Uri"/></param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="CommunicationException">Failed to execute http post</exception>
        public virtual async Task<HttpResponseMessage> PutDataAsync(Uri uri, HttpContent content = null)
        {
            ValidateConnection(uri);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            try
            {
                Log.Info($"PutDataAsync url: {uri.AbsoluteUri}");
                responseMessage = await _client.PutAsync(uri, content ?? new StringContent(string.Empty)).ConfigureAwait(false);
                RecordSuccess();
                return responseMessage;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    RecordFailure();
                    throw new CommunicationException("Failed to execute http PUT request", uri.ToString(), responseMessage.StatusCode, ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of DELETE request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="CommunicationException">Failed to execute http post</exception>
        public virtual async Task<HttpResponseMessage> DeleteDataAsync(Uri uri)
        {
            ValidateConnection(uri);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            try
            {
                Log.Info($"DeleteDataAsync url: {uri.AbsoluteUri}");
                responseMessage = await _client.DeleteAsync(uri).ConfigureAwait(false);
                RecordSuccess();
                return responseMessage;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    RecordFailure();
                    throw new CommunicationException("Failed to execute http DELETE request", uri.ToString(), responseMessage.StatusCode, ex);
                }
                throw;
            }
        }
    }
}
