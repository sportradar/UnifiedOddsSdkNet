﻿using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    public class LockManager
    {
        private readonly ILog _log = SdkLoggerFactory.GetLoggerForExecution(typeof(LockManager));
        private readonly ConcurrentDictionary<string, DateTime> _uniqueItems;
        private readonly TimeSpan _lockTimeout;
        private readonly TimeSpan _lockSleep;
        private bool _waitAll;

        public LockManager(ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            _uniqueItems = uniqueItems ?? new ConcurrentDictionary<string, DateTime>();
            _lockTimeout = lockTimeout.TotalSeconds < 1 ? TimeSpan.FromSeconds(60) : lockTimeout;
            _lockSleep = lockSleep.TotalMilliseconds < 10 ? TimeSpan.FromMilliseconds(50) : lockSleep;
            _waitAll = false;
        }

        public LockManager()
        {
            _uniqueItems = new ConcurrentDictionary<string, DateTime>();
            _lockTimeout = TimeSpan.FromSeconds(60);
            _lockSleep = TimeSpan.FromMilliseconds(50);
            _waitAll = false;
        }

        public void Wait()
        {
            _waitAll = true;
            LockInternal("all", _uniqueItems, _lockTimeout, _lockSleep);
        }

        public void Wait(string key)
        {
            LockInternal(key, _uniqueItems, _lockTimeout, _lockSleep);
        }

        public void Wait(string key, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = _lockTimeout;
            }

            if (lockSleep == TimeSpan.Zero)
            {
                lockSleep = _lockSleep;
            }

            LockInternal(key, _uniqueItems, lockTimeout, lockSleep);
        }

        public void Wait(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (uniqueItems == null)
            {
                throw new ArgumentNullException(nameof(uniqueItems));
            }

            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = _lockTimeout;
            }

            if (lockSleep == TimeSpan.Zero)
            {
                lockSleep = _lockSleep;
            }

            LockInternal(key, uniqueItems, lockTimeout, lockSleep);
        }

        public void Release()
        {
            _waitAll = false;
            ReleaseInternal("all", _uniqueItems, _lockTimeout, _lockSleep);
        }

        public void Release(string key)
        {
            ReleaseInternal(key, _uniqueItems, _lockTimeout, _lockSleep);
        }

        public void Release(string key, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = _lockTimeout;
            }

            if (lockSleep == TimeSpan.Zero)
            {
                lockSleep = _lockSleep;
            }

            ReleaseInternal(key, _uniqueItems, lockTimeout, lockSleep);
        }

        public void Release(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (uniqueItems == null)
            {
                throw new ArgumentNullException(nameof(uniqueItems));
            }

            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = _lockTimeout;
            }

            if (lockSleep == TimeSpan.Zero)
            {
                lockSleep = _lockSleep;
            }

            ReleaseInternal(key, uniqueItems, lockTimeout, lockSleep);
        }

        private void LockInternal(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (!uniqueItems.ContainsKey(key))
            {
                //Debug.WriteLine($"AddOrUpdate key {key} - {uniqueItems.Count}");
                uniqueItems.AddOrUpdate(key, DateTime.Now, UpdateValueFactory);
                return;
            }

            var stopWatch = Stopwatch.StartNew();
            while (_waitAll || (uniqueItems.ContainsKey(key) && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds))
            {
                Task.Delay(lockSleep).Wait();
                //Debug.WriteLine($"Waiting for free processing for key {key} took {stopWatch.ElapsedMilliseconds} ms. {uniqueItems.Count}");
            }

            if (stopWatch.ElapsedMilliseconds > lockTimeout.TotalMilliseconds)
            {
                _log.Warn($"Waiting for end of processing for key {key} took {stopWatch.ElapsedMilliseconds} ms.");
            }

            uniqueItems.AddOrUpdate(key, DateTime.Now, UpdateValueFactory);
        }

        private void ReleaseInternal(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            var stopWatch = Stopwatch.StartNew();
            while (uniqueItems.ContainsKey(key) && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds)
            {
                if (uniqueItems.TryRemove(key, out _))
                {
                    return;
                }
                Task.Delay(lockSleep).Wait();
                //Debug.WriteLine($"Waiting for release for key {key} took {stopWatch.ElapsedMilliseconds} ms.");
            }

            if (stopWatch.ElapsedMilliseconds > lockTimeout.TotalMilliseconds)
            {
                _log.Warn($"Waiting for end of processing release for key {key} took {stopWatch.ElapsedMilliseconds} ms.");
            }
        }

        public void Clean()
        {
            Clean(_uniqueItems, _lockTimeout);
        }

        public void Clean(ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout)
        {
            try
            {
                foreach (var item in uniqueItems)
                {
                    if ((DateTime.Now - item.Value).TotalSeconds > lockTimeout.TotalSeconds)
                    {
                        uniqueItems.TryRemove(item.Key, out _);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Warn("Error cleaning ids for locks.", e);
            }
        }

        private DateTime UpdateValueFactory(string arg1, DateTime arg2)
        {
            return arg2;
        }
    }
}
