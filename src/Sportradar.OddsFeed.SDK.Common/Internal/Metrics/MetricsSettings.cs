using Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    internal static class MetricsSettings
    {
        /// <summary>
        /// Default context for metrics - UFGlobalEventProcessor
        /// </summary>
        public const string McUfSemaphorePool = "UfSdkSemaphorePool";
        public static readonly Timer TimerSemaphorePool = Metric.Context(McUfSemaphorePool).Timer("SemaphorePoolAcquire", Unit.Items);
    }
}
