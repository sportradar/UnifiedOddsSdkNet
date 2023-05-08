﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Dawn;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A class used to connect to the RabbitMQ broker
    /// </summary>
    /// <seealso cref="IRabbitMqChannel" />
    internal class RabbitMqChannel : IRabbitMqChannel
    {
        /// <summary>
        /// The <see cref="ILog"/> used execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(RabbitMqChannel));

        /// <summary>
        /// A <see cref="IChannelFactory"/> used to construct the <see cref="IModel"/> representing Rabbit MQ channel
        /// </summary>
        private readonly IChannelFactory _channelFactory;

        /// <summary>
        /// The <see cref="IModel"/> representing the channel to the broker
        /// </summary>
        private IModel _channel;

        /// <summary>
        /// The <see cref="EventingBasicConsumer"/> used to consume the broker data
        /// </summary>
        private EventingBasicConsumer _consumer;

        /// <summary>
        /// Value indicating whether the current instance is opened. 1 means opened, 0 means closed
        /// </summary>
        private long _isOpened;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RabbitMqMessageReceiver"/> is currently opened;
        /// </summary>
        public bool IsOpened => Interlocked.Read(ref _isOpened) == 1;

        /// <summary>
        /// Occurs when the current channel received the data
        /// </summary>
        public event EventHandler<BasicDeliverEventArgs> Received;

        /// <summary>
        /// The message interest associated by the channel using this instance
        /// </summary>
        private MessageInterest _interest;

        private List<string> _routingKeys;

        private DateTime _channelStarted;

        private DateTime _lastMessageReceived;

        /// <summary>
        /// A <see cref="ITimer"/> instance used to trigger periodic connection test
        /// </summary>
        private readonly ITimer _timer;

        private readonly TimeSpan _maxTimeBetweenMessages;

        private readonly string _accessToken;

        /// <summary>
        /// The timer semaphore slim used reduce concurrency within timer calls
        /// </summary>
        private readonly SemaphoreSlim _timerSemaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqChannel"/> class.
        /// </summary>
        /// <param name="channelFactory">A <see cref="Sportradar.OddsFeed.SDK.API.Internal.IChannelFactory"/> used to construct the <see cref="IModel"/> representing Rabbit MQ channel.</param>
        /// <param name="timer">Timer used to check if there is connection</param>
        /// <param name="maxTimeBetweenMessages">Max timeout between messages to check if connection is ok</param>
        /// <param name="accessToken">The access token to filter from logs</param>
        public RabbitMqChannel(IChannelFactory channelFactory, ITimer timer, TimeSpan maxTimeBetweenMessages, string accessToken)
        {
            Guard.Argument(channelFactory, nameof(channelFactory)).NotNull();

            _channelFactory = channelFactory;
            _routingKeys = new List<string>();
            _lastMessageReceived = DateTime.MinValue;
            _lastMessageReceived = DateTime.MinValue;

            _timer = timer;
            _maxTimeBetweenMessages = maxTimeBetweenMessages;
            _accessToken = accessToken;
            _timer.Elapsed += OnTimerElapsed;
        }

        /// <summary>
        /// Opens the current channel and binds the created queue to provided routing keys
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session using this instance</param>
        /// <param name="routingKeys">A <see cref="IEnumerable{String}"/> specifying the routing keys of the constructed queue.</param>
        public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
        {
            if (Interlocked.CompareExchange(ref _isOpened, 1, 0) != 0)
            {
                ExecutionLog.Error("Opening an already opened channel is not allowed");
                throw new InvalidOperationException("The instance is already opened");
            }

            _interest = interest;
            _routingKeys = routingKeys.ToList();
            if (_routingKeys == null && !_routingKeys.Any())
            {
                throw new ArgumentNullException(nameof(routingKeys), "Missing routing keys");
            }

            _timer.Start();
        }

        /// <summary>
        /// Closes the current channel
        /// </summary>
        /// <exception cref="InvalidOperationException">The instance is already closed</exception>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 0, 1) != 1)
            {
                ExecutionLog.Error($"Cannot close the channel on channelNumber: {_channel?.ChannelNumber}, because this channel is already closed");
                throw new InvalidOperationException("The instance is already closed");
            }

            DetachEvents();

            _timer.Stop();
            _timerSemaphoreSlim.ReleaseSafe();
            _timerSemaphoreSlim.Dispose();
        }

        /// <summary>
        /// Handles the <see cref="EventingBasicConsumer.Received"/> event
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the instance raising the event.</param>
        /// <param name="basicDeliverEventArgs">The <see cref="BasicDeliverEventArgs"/> instance containing the event data.</param>
        private void ConsumerOnDataReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            _lastMessageReceived = DateTime.Now;
            Received?.Invoke(this, basicDeliverEventArgs);
        }

        private void ConsumerOnShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            var reason = _consumer.ShutdownReason == null ? string.Empty : SdkInfo.ClearSensitiveData(_consumer.ShutdownReason.ToString(), _accessToken);
            ExecutionLog.Info($"The consumer {_consumer.ConsumerTag} is shutdown. Reason={reason}");
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            var reason = _channel.CloseReason == null ? string.Empty : SdkInfo.ClearSensitiveData(_channel.CloseReason.ToString(), _accessToken);
            ExecutionLog.Info($"The channel with channelNumber {_channel.ChannelNumber} is shutdown. Reason={reason}");
        }

        private void CreateAndAttachEvents()
        {
            ExecutionLog.Info("Opening the channel ...");
            _channel = _channelFactory.CreateChannel();
            ExecutionLog.Info($"Channel opened with channelNumber: {_channel.ChannelNumber}. MaxAllowedTimeBetweenMessages={_maxTimeBetweenMessages.TotalSeconds}s.");
            var declareResult = _channel.QueueDeclare();

            foreach (var routingKey in _routingKeys)
            {
                ExecutionLog.Info($"Binding queue={declareResult.QueueName} with routingKey={routingKey}");
                _channel.QueueBind(declareResult.QueueName, "unifiedfeed", routingKey);
            }

            var interestName = _interest == null ? "system" : _interest.Name;
            _channel.ModelShutdown += ChannelOnModelShutdown;
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.ConsumerTag = $"UfSdk-Net|{SdkInfo.GetVersion()}|{interestName}|{_channel.ChannelNumber}|{DateTime.Now:yyyyMMdd-HHmmss}|{SdkInfo.GetGuid(8)}";
            _consumer.Received += ConsumerOnDataReceived;
            _consumer.Shutdown += ConsumerOnShutdown;
            _channel.BasicConsume(declareResult.QueueName, true, _consumer.ConsumerTag, _consumer);
            ExecutionLog.Info($"BasicConsume for channel={_channel.ChannelNumber}, queue={declareResult.QueueName} and consumer tag {_consumer.ConsumerTag} executed.");

            _lastMessageReceived = DateTime.MinValue;
            _channelStarted = DateTime.Now;
        }

        private void DetachEvents()
        {
            ExecutionLog.Info($"Closing the channel with channelNumber: {_channel?.ChannelNumber}");
            if (_consumer != null)
            {
                _consumer.Received -= ConsumerOnDataReceived;
                _consumer.Shutdown -= ConsumerOnShutdown;
                _consumer = null;
            }
            if (_channel != null)
            {
                _channel.ModelShutdown -= ChannelOnModelShutdown;
                _channel.Dispose();
                _channel = null;
            }

            _channelStarted = DateTime.MinValue;
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                await OnTimerElapsedAsync().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Invoked when the internally used timer elapses
        /// </summary>
        private async Task OnTimerElapsedAsync()
        {
            await _timerSemaphoreSlim.WaitAsync().ConfigureAwait(false);

            if (IsOpened)
            {
                try
                {
                    //ExecutionLog.Info($"Connection created={_channelFactory.ConnectionCreated}, ConnectionOpened={_channelFactory.IsConnectionOpen()}, Channel={_channel?.ChannelNumber}, ChannelOpened={_channel?.IsOpen}");
                    if (_channel == null)
                    {
                        ExecutionLog.Info("Creating connection channel and attaching events ...");
                        CreateAndAttachEvents();
                    }

                    // it means, the connection was reset in between
                    if (_channelFactory.ConnectionCreated > _channelStarted)
                    {
                        ExecutionLog.Info("Recreating connection channel and attaching events ...");
                        DetachEvents();
                        CreateAndAttachEvents();
                    }

                    // no messages arrived in last _maxTimeBetweenMessages seconds, from the start of the channel
                    var channelStartedDiff = DateTime.Now - _channelStarted;
                    if (_lastMessageReceived == DateTime.MinValue && _channelStarted > DateTime.MinValue && channelStartedDiff > _maxTimeBetweenMessages)
                    {
                        var isOpen = _channelFactory.IsConnectionOpen() ? "s" : string.Empty;
                        ExecutionLog.Warn($"There were no message{isOpen} in more then {_maxTimeBetweenMessages.TotalSeconds}s for the channel with channelNumber: {_channel?.ChannelNumber}. Last message arrived: {_lastMessageReceived}. Recreating channel.");
                        DetachEvents();
                        CreateAndAttachEvents();
                    }

                    // we have received messages in the past, but not in last _maxTimeBetweenMessages seconds
                    var lastMessageDiff = DateTime.Now - _lastMessageReceived;
                    if (_lastMessageReceived > DateTime.MinValue && lastMessageDiff > _maxTimeBetweenMessages)
                    {
                        var isOpen = _channelFactory.IsConnectionOpen() ? "s" : string.Empty;
                        ExecutionLog.Warn($"There were no message{isOpen} in more then {_maxTimeBetweenMessages.TotalSeconds}s for the channel with channelNumber: {_channel?.ChannelNumber}. Last message arrived: {_lastMessageReceived}");
                        DetachEvents();
                        if (_channelFactory.ConnectionCreated < _channelStarted)
                        {
                            ExecutionLog.Info($"Resetting connection for the channel with channelNumber: {_channel?.ChannelNumber}");
                            _channelFactory.ResetConnection();
                            ExecutionLog.Info($"Resetting connection finished for the channel with channelNumber: {_channel?.ChannelNumber}");
                        }
                        CreateAndAttachEvents();
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.Warn($"Error checking connection for channelNumber: {_channel?.ChannelNumber}", e);
                }
            }

            _timerSemaphoreSlim.ReleaseSafe();
        }
    }
}
