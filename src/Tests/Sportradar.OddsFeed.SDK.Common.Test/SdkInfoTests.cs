using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class SdkInfoTests
    {
        [TestMethod]
        public void MinInactivitySecondsPositive()
        {
            Assert.IsTrue(SdkInfo.MinInactivitySeconds > 0);
        }

        [TestMethod]
        public void MaxInactivitySecondsPositive()
        {
            Assert.IsTrue(SdkInfo.MaxInactivitySeconds > 0);
        }

        [TestMethod]
        public void MinInactivitySecondsLessThenMaxInactivitySeconds()
        {
            Assert.IsTrue(SdkInfo.MinInactivitySeconds < SdkInfo.MaxInactivitySeconds);
        }

        [TestMethod]
        public void DecimalToStringWithSignReturnsCorrect()
        {
            Assert.AreEqual("0", SdkInfo.DecimalToStringWithSign(0));
            Assert.AreEqual("+1", SdkInfo.DecimalToStringWithSign(1));
            Assert.AreEqual("-1", SdkInfo.DecimalToStringWithSign(-1));
            Assert.AreEqual("+0.05", SdkInfo.DecimalToStringWithSign(0.05M));
            Assert.AreEqual("-1.01", SdkInfo.DecimalToStringWithSign(-1.01M));
            Assert.AreEqual("+1.25", SdkInfo.DecimalToStringWithSign(1.25M));
        }

        [TestMethod]
        public void GetRandomReturnsInt()
        {
            var value = SdkInfo.GetRandom();
            Assert.IsTrue(value >= 0);
        }

        [TestMethod]
        public void GetRandomReturnsCorrectBetweenMinMax()
        {
            const int min = 100;
            const int max = 1000;
            for (var i = 0; i < 10000; i++)
            {
                var value = SdkInfo.GetRandom(min, max);
                Assert.IsTrue(value >= min, $"{value} must be greater then {min}");
                Assert.IsTrue(value < max, $"{value} must be less then {max}");
            }
        }

        [TestMethod]
        public void GetVariableNumberForIntReturnsVariableBetweenMinMax()
        {
            const int baseValue = 100;
            const int variablePercent = 10;
            const double min = ((100 - (double)variablePercent) / 100) * baseValue;
            const double max = ((100 + (double)variablePercent) / 100) * baseValue;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.GetVariableNumber(baseValue, variablePercent);
                Assert.IsTrue(value >= min, $"{value} must be greater then {min}");
                Assert.IsTrue(value < max, $"{value} must be less then {max}");
            }
        }

        [TestMethod]
        public void GetVariableNumberForTimeSpanReturnsVariableBetweenMinMax()
        {
            var baseValue = TimeSpan.FromSeconds(100);
            const int variablePercent = 10;
            var min = ((100 - (double)variablePercent) / 100) * baseValue.TotalSeconds;
            var max = ((100 + (double)variablePercent) / 100) * baseValue.TotalSeconds;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.GetVariableNumber(baseValue, variablePercent);
                Assert.IsTrue(value.TotalSeconds >= min, $"{value.TotalSeconds} must be greater then {min}");
                Assert.IsTrue(value.TotalSeconds < max, $"{value.TotalSeconds} must be less then {max}");
            }
        }

        [TestMethod]
        public void AddVariableNumberForTimeSpanReturnsVariableBetweenMinMax()
        {
            var baseValue = TimeSpan.FromSeconds(100);
            const int variablePercent = 10;
            var max = ((100 + (double)variablePercent) / 100) * baseValue.TotalSeconds;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.AddVariableNumber(baseValue, variablePercent);
                Assert.IsTrue(value.TotalSeconds >= baseValue.TotalSeconds, $"{value.TotalSeconds} must be greater then {baseValue.TotalSeconds}");
                Assert.IsTrue(value.TotalSeconds < max, $"{value.TotalSeconds} must be less then {max}");
            }
        }

        [TestMethod]
        public void AddVariableNumberForProfileCacheTimeoutReturnsVariableBetweenMinMax()
        {
            var baseValue = OperationManager.ProfileCacheTimeout;
            const int variablePercent = 10;
            var max = ((100 + (double)variablePercent) / 100) * baseValue.TotalSeconds;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.AddVariableNumber(baseValue, variablePercent);
                Assert.IsTrue(value.TotalSeconds >= baseValue.TotalSeconds, $"{value.TotalSeconds} must be greater then {baseValue.TotalSeconds}");
                Assert.IsTrue(value.TotalSeconds < max, $"{value.TotalSeconds} must be less then {max}");
                Assert.IsTrue(value.TotalSeconds < baseValue.TotalSeconds * 1.1, $"{value.TotalSeconds} must be less then {baseValue.TotalSeconds * 1.1}");
            }
        }

        [TestMethod]
        public void CultureInfoTest()
        {
            const int iteration = 1000000;
            var cultureList = new List<CultureInfo>();
            for (var i = 0; i < iteration; i++)
            {
                cultureList.Add(new CultureInfo("en"));
            }

            for (var i = 0; i < 1000; i++)
            {
                var r1 = SdkInfo.GetRandom(iteration);
                var r2 = SdkInfo.GetRandom(iteration);
                var culture1 = cultureList[r1];
                var culture2 = cultureList[r2];
                Assert.IsNotNull(culture1);
                Assert.IsNotNull(culture2);
                Assert.AreEqual(culture1, culture2);
                //Assert.AreSame(culture1, culture2);
            }
        }

        [TestMethod]
        public void CultureInfoAsStringTest()
        {
            const int iteration = 1000000;
            var cultureList = new List<string>();
            for (var i = 0; i < iteration; i++)
            {
                cultureList.Add("en");
            }

            for (var i = 0; i < 1000; i++)
            {
                var r1 = SdkInfo.GetRandom(iteration);
                var r2 = SdkInfo.GetRandom(iteration);
                var culture1 = cultureList[r1];
                var culture2 = cultureList[r2];
                Assert.IsNotNull(culture1);
                Assert.IsNotNull(culture2);
                Assert.AreEqual(culture1, culture2);
                Assert.AreSame(culture1, culture2);
            }
        }

        [TestMethod]
        public void CultureInfoInDictionaryTest()
        {
            const int iteration = 100000;
            var dictList = new List<Dictionary<CultureInfo, string>>();
            var cultureList = new Dictionary<CultureInfo, string>();
            for (var i = 0; i < iteration; i++)
            {
                cultureList.Clear();
                cultureList.Add(new CultureInfo("en"), SdkInfo.GetGuid(20));
                dictList.Add(cultureList);
            }

            for (var i = 0; i < 1000; i++)
            {
                var r1 = SdkInfo.GetRandom(iteration);
                var r2 = SdkInfo.GetRandom(iteration);
                var culture1 = dictList[r1].Keys.First();
                var culture2 = dictList[r2].Keys.First();
                Assert.IsNotNull(culture1);
                Assert.IsNotNull(culture2);
                Assert.AreEqual(culture1, culture2);
                Assert.AreSame(culture1, culture2);
            }
        }
    }
}
