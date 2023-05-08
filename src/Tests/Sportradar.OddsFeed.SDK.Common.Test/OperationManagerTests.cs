using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class OperationManagerTests
    {
        private const int DefaultSportEventStatusCacheTimeout = 5;
        private const int DefaultProfileCacheTimeout = 24;
        private const int DefaultVariantMarketDescriptionCacheTimeout = 3;
        private const int DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout = 3;

        [TestMethod]
        public void Initialization()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Assert.AreEqual(false, OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        [TestMethod]
        public void SportEventStatusCacheTimeout()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(30));
            Assert.AreEqual(30, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [TestMethod]
        public void SportEventStatusCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(1));
            Assert.AreEqual(1, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [TestMethod]
        public void SportEventStatusCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(60));
            Assert.AreEqual(60, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SportEventStatusCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SportEventStatusCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(61));
        }

        [TestMethod]
        public void ProfileCacheTimeout()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(30));
            Assert.AreEqual(30, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void ProfileCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(1));
            Assert.AreEqual(1, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void ProfileCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(48));
            Assert.AreEqual(48, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ProfileCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ProfileCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(49));
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheTimeout()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(10));
            Assert.AreEqual(10, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(1));
            Assert.AreEqual(1, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(24));
            Assert.AreEqual(24, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void VariantMarketDescriptionCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void VariantMarketDescriptionCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(25));
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeout()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(10));
            Assert.AreEqual(10, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(1));
            Assert.AreEqual(1, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(24));
            Assert.AreEqual(24, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(25));
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatus()
        {
            ResetOperationManager();
            Assert.IsFalse(OperationManager.IgnoreBetPalTimelineSportEventStatus);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatus(true);
            Assert.IsTrue(OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        [TestMethod]
        public void SetMaxConnectionsPerServerSavedValidNumber()
        {
            const int newMaxConnections = 1000;
            ResetOperationManager();
            Assert.AreEqual(20, OperationManager.MaxConnectionsPerServer);
            OperationManager.SetMaxConnectionsPerServer(newMaxConnections);
            Assert.AreEqual(newMaxConnections, OperationManager.MaxConnectionsPerServer);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SetMaxConnectionsPerServerInvalidNumberThrows()
        {
            const int newMaxConnections = 0;
            Assert.AreEqual(20, OperationManager.MaxConnectionsPerServer);
            OperationManager.SetMaxConnectionsPerServer(newMaxConnections);
        }

        private void ResetOperationManager()
        {
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(DefaultSportEventStatusCacheTimeout));
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(DefaultProfileCacheTimeout));
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(DefaultVariantMarketDescriptionCacheTimeout));
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout));
            OperationManager.SetIgnoreBetPalTimelineSportEventStatus(false);
        }
    }
}
