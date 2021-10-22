using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Class EnvironmentSetting
    /// </summary>
    public class EnvironmentSetting
    {
        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>The environment</value>
        public SdkEnvironment Environment { get; }

        /// <summary>
        /// Gets the rabbit host address
        /// </summary>
        /// <value>The rabbit host address</value>
        public string MqHost { get; }

        /// <summary>
        /// Gets the API host.
        /// </summary>
        /// <value>The API host.</value>
        public string ApiHost { get; }

        /// <summary>
        /// Gets a value indicating whether only SSL is supported on the endpoint or also non-ssl
        /// </summary>
        /// <value><c>true</c> if [only SSL]; otherwise, <c>false</c>.</value>
        public bool OnlySsl { get; }

        /// <summary>
        /// Gets the environment retry list.
        /// </summary>
        /// <value>The environment retry list.</value>
        public List<SdkEnvironment> EnvironmentRetryList { get; }

        public EnvironmentSetting(SdkEnvironment environment, string mqHost, string apiHost, bool onlySsl, List<SdkEnvironment> environmentRetryList = null)
        {
            Environment = environment;
            MqHost = mqHost;
            ApiHost = apiHost;
            OnlySsl = onlySsl;
            EnvironmentRetryList = environmentRetryList ?? new List<SdkEnvironment>();
        }
    }
}
