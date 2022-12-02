using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class SdkTimerTests
    {
        private ITimer _sdkTimer;
        private IList<string> _timerMsgs;

        [TestInitialize]
        public void InitSdkTimerTests()
        {
            ThreadPool.SetMinThreads(100, 100);
            _timerMsgs = new List<string>();
            _sdkTimer = new SdkTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void SdkTimerOnElapsed(object sender, EventArgs e)
        {
            _timerMsgs.Add($"{_timerMsgs.Count + 1}. message");
        }

        [TestMethod]
        public void TimerNormalInitializationTest()
        {
            Assert.IsNotNull(_sdkTimer);
            Assert.IsFalse(_timerMsgs.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TimerWrongDueTimeTest()
        {
            SdkTimer.Create(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TimerWrongPeriodTimeTest()
        {
            SdkTimer.Create(TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TimerStartWrongDueTimeTest()
        {
            _sdkTimer.Start(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TimerStartWrongPeriodTimeTest()
        {
            _sdkTimer.Start(TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [TestMethod]
        public async Task TimerNormalOperationTest()
        {
            _sdkTimer.Elapsed += SdkTimerOnElapsed;
            _sdkTimer.Start();
            await Task.Delay(5200).ConfigureAwait(false);
            Assert.IsNotNull(_sdkTimer);
            Assert.IsTrue(_timerMsgs.Any());
            Assert.IsTrue(5 <= _timerMsgs.Count, $"Expected 5, actual {_timerMsgs.Count}");
        }

        [TestMethod]
        public async Task TimerFailedOnTickWillNotBreakTest()
        {
            _sdkTimer.Elapsed += (sender, args) =>
                                 {
                                     _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
                                     throw new InvalidOperationException("Some error");
                                 };
            _sdkTimer.Start();
            await Task.Delay(1200).ConfigureAwait(false);
            Assert.IsNotNull(_sdkTimer);
            Assert.IsTrue(_timerMsgs.Any());
            Assert.AreEqual(1, _timerMsgs.Count);
        }

        [TestMethod]
        public async Task TimerFailedOnTickWillContinueOnPeriodTest()
        {
            _sdkTimer.Elapsed += (sender, args) =>
                                 {
                                     _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
                                     throw new InvalidOperationException("Some error");
                                 };
            _sdkTimer.Start();
            await Task.Delay(5200).ConfigureAwait(false);
            Assert.IsNotNull(_sdkTimer);
            Assert.IsTrue(_timerMsgs.Any());
            Assert.IsTrue(5 <= _timerMsgs.Count, $"Expected 5, actual {_timerMsgs.Count}");
        }

        [TestMethod]
        public async Task TimerFireOnceTest()
        {
            var sdkTimer = new SdkTimer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
            sdkTimer.Elapsed += SdkTimerOnElapsed;
            sdkTimer.FireOnce(TimeSpan.Zero);
            await Task.Delay(3200).ConfigureAwait(false);
            Assert.IsNotNull(_sdkTimer);
            Assert.IsTrue(_timerMsgs.Any());
            Assert.AreEqual(1, _timerMsgs.Count);
        }
    }
}
