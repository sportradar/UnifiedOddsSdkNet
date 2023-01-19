using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class EnvironmentManagerTests
    {
        [TestMethod]
        public void GlobalAndNonGlobalApiSitUnderSameIpsButReplayShouldPointToNonGlobalAsItIsLongTermStrategy()
        {
            const string nonGlobalApiHost = "stgapi.betradar.com";
            Assert.AreEqual(nonGlobalApiHost, FindReplayEnvironmentSetting().ApiHost);
            Assert.AreEqual(nonGlobalApiHost, EnvironmentManager.GetSetting(SdkEnvironment.Replay).ApiHost);
            Assert.AreEqual(nonGlobalApiHost, EnvironmentManager.GetApiHost(SdkEnvironment.Replay));
        }

        [TestMethod]
        public void ReplayShouldPointToNonGlobalMessagingEndpointAsItIsLongTermStrategy()
        {
            const string nonGlobalMessagingHost = "replaymq.betradar.com";
            Assert.AreEqual(nonGlobalMessagingHost, FindReplayEnvironmentSetting().MqHost);
            Assert.AreEqual(nonGlobalMessagingHost, EnvironmentManager.GetSetting(SdkEnvironment.Replay).MqHost);
            Assert.AreEqual(nonGlobalMessagingHost, EnvironmentManager.GetMqHost(SdkEnvironment.Replay));
        }

        [TestMethod]
        public void ReplayShouldSupportSslOnlyConnections()
        {
            Assert.IsTrue(FindReplayEnvironmentSetting().OnlySsl);
            Assert.IsTrue(EnvironmentManager.GetSetting(SdkEnvironment.Replay).OnlySsl);
        }

        [TestMethod]
        public void RetryListShouldBeDeprecatedAsItIsNotOnlyNeverUsedInReplayButItIsExposedThroughStaticContextToCustomerSusceptableToBreakingChange()
        {
            Assert.IsNotNull(FindReplayEnvironmentSetting().EnvironmentRetryList);
            Assert.IsNotNull(EnvironmentManager.GetSetting(SdkEnvironment.Replay).EnvironmentRetryList);
        }

        private EnvironmentSetting FindReplayEnvironmentSetting()
        {
            var replaySettings = EnvironmentManager.EnvironmentSettings.Find(e => e.Environment == SdkEnvironment.Replay);
            Assert.IsNotNull(replaySettings);
            return replaySettings;
        }
    }
}
