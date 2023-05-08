/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    /// <summary>
    /// Result are in log file
    /// </summary>
    [TestClass]
    public class LogProxyTest
    {
        private DemoMethods _demoClass;

        [TestInitialize]
        public void Init()
        {
            _demoClass = LogProxyFactory.Create<DemoMethods>(null, m => m.Name.Contains("D"), LoggerType.ClientInteraction);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfVoidMethod()
        {
            _demoClass.DemoVoidMethod();
            var res = _demoClass.DemoIntMethod(10, 50);
            Assert.IsTrue(res > 100);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfAsyncCallerMethod()
        {
            var res = _demoClass.DemoLongLastingMethodAsyncCaller(45, 10);
            Assert.AreEqual(100, res);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfAsyncMethod()
        {
            var res = _demoClass.DemoLongLastingMethodAsync(10, 25).GetAwaiter().GetResult();
            res = res + _demoClass.DemoLongLastingMethodAsync(15, 20).GetAwaiter().GetResult();
            res = res + _demoClass.DemoLongLastingMethodAsync(40, 15).GetAwaiter().GetResult();
            res = res + _demoClass.DemoLongLastingMethodAsync(40, 10).GetAwaiter().GetResult();
            Assert.IsTrue(res > 100);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfAsyncGroup()
        {
            var res = 120;
            Task.Run(async () =>
            {
                var res1 = await _demoClass.DemoLongLastingMethodAsync(10, 25);
                await _demoClass.DemoLongLastingMethodAsync(15, 20);
                await _demoClass.DemoLongLastingMethodAsync(40, 15);
                await _demoClass.DemoLongLastingMethodAsync(40, 10);
                res = res1;
            }).GetAwaiter().GetResult();

            Assert.IsTrue(res > 100);
        }

        [TestMethod]
        public void LogInputAndOutParametersOfGroupAsyncMethod()
        {
            var tasks = new List<Task>
            {
                _demoClass.DemoLongLastingMethodAsync(10, 25),
                _demoClass.DemoLongLastingMethodAsync(15, 20),
                _demoClass.DemoLongLastingMethodAsync(40, 15),
                _demoClass.DemoLongLastingMethodAsync(40, 10)
            };
            Task.Run(async () =>
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }).GetAwaiter().GetResult();

            var res = tasks[0].Id;
            Assert.IsTrue(res > 0);
        }

        [TestMethod]
        public void LogInAndOutParamsOfGroupAsyncMethod()
        {
            var res = 120;
            Task.Run(async () =>
            {
                var res1 = await _demoClass.DemoLongLastingMethodAsync(45, 10);
                res = res1;
            }).GetAwaiter().GetResult();

            Assert.AreEqual(100, res);
        }

        [TestMethod]
        public void LogInAndOutParamsOfCustomMethod()
        {
            const int res = 100;
            Task.Run(async () =>
            {
                await _demoClass.DemoCustomMethodAsync(450);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(100, res);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void MethodWithTaskThrowsExceptionAsync()
        {
            var res = 100;
            Task.Run(async () =>
            {
                res = await _demoClass.DemoMethodWithTaskThrowsExceptionAsync(450);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(100, res);
        }
    }
}
