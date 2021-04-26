﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Dawn;
using Metrics;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using DateTime = System.DateTime;
// ReSharper disable InconsistentlySynchronizedField

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    internal class EventChangeManager : IEventChangeManager
    {
        private static readonly ILog LogInt = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(EventChangeManager));
        private static readonly ILog LogExec = SdkLoggerFactory.GetLoggerForExecution(typeof(EventChangeManager));

        public event EventHandler<EventChangeEventArgs> FixtureChange;

        public event EventHandler<EventChangeEventArgs> ResultChange;

        public DateTime LastFixtureChange { get; private set; }

        public DateTime LastResultChange { get; private set; }

        public TimeSpan FixtureChangeInterval { get; private set; }

        public TimeSpan ResultChangeInterval { get; private set; }

        public bool IsRunning { get; private set; }

        private readonly SdkTimer _fixtureTimer;
        private readonly SdkTimer _resultTimer;
        private readonly SportDataProvider _sportDataProvider;
        private readonly ISportEventCache _sportEventCache;
        private readonly IOddsFeedConfiguration _config;
        private const string MetricsContext = "EventChangeManager";
        private readonly object _lockFixture = new object();
        private readonly object _lockResult = new object();
        private bool _isDispatching;
        private readonly ConcurrentDictionary<EventChangeEventArgs, bool> _eventUpdates; // boolean value indicating if this is fixture update

        public EventChangeManager(TimeSpan fixtureChangeInterval, 
                                  TimeSpan resultChangeInterval,
                                  ISportDataProvider sportDataProvider, 
                                  ISportEventCache sportEventCache,
                                  IOddsFeedConfiguration config)
        {   
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(sportDataProvider, nameof(sportDataProvider)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            
            _config = config;
            _sportDataProvider = (SportDataProvider)sportDataProvider;
            _sportEventCache = sportEventCache;

            LastFixtureChange = DateTime.MinValue;
            LastResultChange = DateTime.MinValue;
            FixtureChangeInterval = fixtureChangeInterval >= TimeSpan.FromMinutes(1) ? fixtureChangeInterval : TimeSpan.FromHours(1);
            ResultChangeInterval = resultChangeInterval >= TimeSpan.FromMinutes(1) ? resultChangeInterval : TimeSpan.FromHours(1);
            IsRunning = false;

            _fixtureTimer = new SdkTimer(TimeSpan.FromSeconds(1), fixtureChangeInterval);
            _resultTimer = new SdkTimer(TimeSpan.FromSeconds(1), ResultChangeInterval);
            _fixtureTimer.Elapsed += FixtureTimerOnElapsed;
            _resultTimer.Elapsed += ResultTimerOnElapsed;

            _eventUpdates = new ConcurrentDictionary<EventChangeEventArgs, bool>();

            _isDispatching = false;
        }

        public void SetFixtureChangeInterval(TimeSpan fixtureChangeInterval)
        {
            if(fixtureChangeInterval < TimeSpan.FromMinutes(1) || fixtureChangeInterval > TimeSpan.FromHours(12))
            {
                throw new ArgumentException("Interval must be between 1 minute and 12 hours", nameof(fixtureChangeInterval));
            }

            LogInt.Info($"Setting new fixture change interval to {fixtureChangeInterval.TotalMinutes} min.");
            FixtureChangeInterval = fixtureChangeInterval;
            if (IsRunning)
            {
                _fixtureTimer.Start(TimeSpan.FromSeconds(1), FixtureChangeInterval);
            }
        }

        public void SetResultChangeInterval(TimeSpan resultChangeInterval)
        {
            if(resultChangeInterval < TimeSpan.FromMinutes(1) || resultChangeInterval > TimeSpan.FromHours(12))
            {
                throw new ArgumentException("Interval must be between 1 minute and 12 hours", nameof(resultChangeInterval));
            }

            LogInt.Info($"Setting new result change interval to {resultChangeInterval.TotalMinutes} min.");
            ResultChangeInterval = resultChangeInterval;
            if (IsRunning)
            {
                _resultTimer.Start(TimeSpan.FromSeconds(1), ResultChangeInterval);
            }
        }

        public void SetFixtureChangeTimestamp(DateTime fixtureChangeTimestamp)
        {
            if (IsRunning)
            {
                throw new ArgumentException("Manager must first be stopped.", nameof(fixtureChangeTimestamp));
            }
            if(fixtureChangeTimestamp < DateTime.Now.AddDays(-1) || fixtureChangeTimestamp > DateTime.Now)
            {
                throw new ArgumentException("Timestamp must be in the last 24 hours.", nameof(fixtureChangeTimestamp));
            }

            LogInt.Info($"Set LastFixtureChange to {fixtureChangeTimestamp}.");
            LastFixtureChange = fixtureChangeTimestamp;
        }

        public void SetResultChangeTimestamp(DateTime resultChangeTimestamp)
        {
            if (IsRunning)
            {
                throw new ArgumentException("Manager must first be stopped.", nameof(resultChangeTimestamp));
            }
            if(resultChangeTimestamp < DateTime.Now.AddDays(-1) || resultChangeTimestamp > DateTime.Now)
            {
                throw new ArgumentException("Timestamp must be in the last 24 hours.", nameof(resultChangeTimestamp));
            }

            LogInt.Info($"Set LastResultChange to {resultChangeTimestamp}.");
            LastResultChange = resultChangeTimestamp;
        }

        public void Start()
        {
            if (!IsRunning)
            {
                LogInt.Info("Starting periodical fetching of fixture and result changes.");
                IsRunning = true;
                _fixtureTimer.Start(TimeSpan.FromSeconds(1), FixtureChangeInterval);
                _resultTimer.Start(TimeSpan.FromSeconds(1), ResultChangeInterval);
            }
            else
            {
                LogInt.Info("Invoking Start of already started process.");
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                LogInt.Info("Stopping periodical fetching of fixture and result changes.");
            }
            _fixtureTimer.Stop();
            _resultTimer.Stop();
            IsRunning = false;
        }

        private void FixtureTimerOnElapsed(object sender, EventArgs e)
        {
            if (!IsRunning)
            {
                LogExec.Debug("Invoked fixture change fetch when IsRunning=false.");
                return;
            }

            if (FixtureChange == null)
            {
                LogExec.Debug("Invoked fixture change fetch when no event handler specified for FixtureChange event. Aborting.");
                return;
            }

            lock (_lockFixture)
            {
                if(!IsRunning)
                {
                    return;
                }

                try
                {
                    Metric.Context(MetricsContext).Meter("FixtureChanges", Unit.Calls);
                    var timer = Metric.Context(MetricsContext).Timer("FixtureChanges", Unit.Requests);
                    using (var t = timer.NewContext())
                    {
                        IEnumerable<IFixtureChange> changes;
                        if (LastFixtureChange < DateTime.Now.AddDays(-1))
                        {
                            LogExec.Info("Invoking GetFixtureChanges. After=null");
                            changes = _sportDataProvider.GetFixtureChangesAsync(_config.DefaultLocale).Result;
                        }
                        else
                        {
                            LogExec.Info($"Invoking GetFixtureChanges. After={LastFixtureChange}");
                            changes = _sportDataProvider.GetFixtureChangesAsync(LastFixtureChange, null, _config.DefaultLocale).Result;
                        }

                        changes = changes.OrderBy(o => o.UpdateTime);

                        foreach (var fixtureChange in changes)
                        {
                            if (!IsRunning)
                            {
                                break;
                            }

                            var eventUpdate = _eventUpdates.FirstOrDefault(a => a.Key.SportEventId.Equals(fixtureChange.SportEventId));
                            if (eventUpdate.Key != null)
                            {
                                if (fixtureChange.UpdateTime > eventUpdate.Key.UpdateTime)
                                {
                                    _eventUpdates.TryRemove(eventUpdate.Key, out bool _);
                                }
                                else
                                {
                                    UpdateLastFixtureChange(fixtureChange.UpdateTime);
                                    continue;
                                }
                            }

                            _sportEventCache.CacheDeleteItem(fixtureChange.SportEventId, CacheItemType.All);
                            var sportEvent = _sportDataProvider.GetSportEventForEventChange(fixtureChange.SportEventId);
                            _eventUpdates.TryAdd(new EventChangeEventArgs(fixtureChange.SportEventId, fixtureChange.UpdateTime, sportEvent), true);
                            UpdateLastFixtureChange(fixtureChange.UpdateTime);
                        }

                        LogExec.Info($"Invoking GetFixtureChanges took {t.Elapsed.TotalSeconds}s.");
                    }
                }
                catch (Exception ex)
                {
                    LogExec.Error($"Error fetching fixture changes. Exception={ex.Message}");
                }
            }

            DispatchUpdateChangeMessages();
        }

        private void ResultTimerOnElapsed(object sender, EventArgs e)
        {
            if (!IsRunning)
            {
                LogExec.Debug("Invoked result change fetch when IsRunning=false.");
                return;
            }

            if (ResultChange == null)
            {
                LogExec.Debug("Invoked result change fetch when no event handler specified for ResultChange event. Aborting.");
                return;
            }

            lock (_lockResult)
            {
                if(!IsRunning)
                {
                    return;
                }

                try
                {
                    Metric.Context(MetricsContext).Meter("ResultChanges", Unit.Calls);
                    var timer = Metric.Context(MetricsContext).Timer("ResultChanges", Unit.Requests);
                    using (var t = timer.NewContext())
                    {
                        IEnumerable<IResultChange> changes;
                        if (LastResultChange < DateTime.Now.AddDays(-1))
                        {
                            LogExec.Info("Invoking GetResultChanges. After=null");
                            changes = _sportDataProvider.GetResultChangesAsync(_config.DefaultLocale).Result;
                        }
                        else
                        {
                            LogExec.Info($"Invoking GetResultChanges. After={LastResultChange}");
                            changes = _sportDataProvider.GetResultChangesAsync(LastResultChange, null, _config.DefaultLocale).Result;
                        }

                        changes = changes.OrderBy(o => o.UpdateTime);

                        foreach (var resultChange in changes)
                        {
                            if (!IsRunning)
                            {
                                break;
                            }

                            var eventUpdate = _eventUpdates.FirstOrDefault(a => a.Key.SportEventId.Equals(resultChange.SportEventId));
                            if (eventUpdate.Key != null)
                            {
                                if (resultChange.UpdateTime > eventUpdate.Key.UpdateTime)
                                {
                                    _eventUpdates.TryRemove(eventUpdate.Key, out bool _);
                                }
                                else
                                {
                                    UpdateLastResultChange(resultChange.UpdateTime);
                                    continue;
                                }
                            }

                            _sportEventCache.CacheDeleteItem(resultChange.SportEventId, CacheItemType.All);
                            var sportEvent = _sportDataProvider.GetSportEventForEventChange(resultChange.SportEventId);
                            _eventUpdates.TryAdd(new EventChangeEventArgs(resultChange.SportEventId, resultChange.UpdateTime, sportEvent), false);
                            UpdateLastResultChange(resultChange.UpdateTime);
                        }

                        LogExec.Info($"Invoking GetResultChanges took {t.Elapsed.TotalSeconds}s.");
                    }
                }
                catch (Exception ex)
                {
                    LogExec.Error($"Error fetching result changes. Exception={ex.Message}");
                }
            }

            DispatchUpdateChangeMessages();
        }

        private void UpdateLastFixtureChange(DateTime newDate)
        {
            if (newDate > LastFixtureChange)
            {
                LastFixtureChange = newDate;
            }
        }

        private void UpdateLastResultChange(DateTime newDate)
        {
            if (newDate > LastResultChange)
            {
                LastResultChange = newDate;
            }
        }

        private void DispatchUpdateChangeMessages()
        {
            if(_isDispatching)
            {
                return;
            }

            _isDispatching = true;
            foreach (var eventUpdate in _eventUpdates)
            {
                var updateStr = eventUpdate.Value ? "fixture" : "result";
                try
                {
                    LogInt.Debug($"Dispatching {updateStr} change [{_eventUpdates.Count}] for {eventUpdate.Key.SportEventId}. Updated={eventUpdate.Key.UpdateTime}");
                    if (eventUpdate.Value)
                    {
                        FixtureChange?.Invoke(this, eventUpdate.Key);
                    }
                    else
                    {
                        ResultChange?.Invoke(this, eventUpdate.Key);
                    }

                    _eventUpdates.TryRemove(eventUpdate.Key, out bool _);
                }
                catch (Exception exception)
                {
                    LogExec.Warn($"Error during user processing of event {updateStr} change message: {exception.Message}");
                }
            }
            _isDispatching = false;
        }
    }
}
