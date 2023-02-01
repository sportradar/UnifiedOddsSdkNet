using Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    internal static class MetricsSettings
    {
        /// <summary>
        /// Default context for UfSdkSemaphorePool
        /// </summary>
        public const string McUfSemaphorePool = "UfSdkSemaphorePool";
        public static readonly Timer TimerSemaphorePool = Metric.Context(McUfSemaphorePool).Timer("SemaphorePoolAcquire", Unit.Items);

        /// <summary>
        /// Default context for UfSdkRabbitMessageReceiver
        /// </summary>
        private const string McUfRabbitMqMessageReceiver = "UfSdkRabbitMessageReceiver";
        public static readonly Timer TimerMessageDeserialize = Metric.Context(McUfRabbitMqMessageReceiver).Timer("Message deserialization time", Unit.Items);
        public static readonly Timer TimerRawFeedDataDispatch = Metric.Context(McUfRabbitMqMessageReceiver).Timer("Raw message dispatched", Unit.Items);
        public static readonly Meter MeterMessageConsume = Metric.Context(McUfRabbitMqMessageReceiver).Meter("Message received", Unit.Items);
        public static readonly Meter MeterMessageDeserializeException = Metric.Context(McUfRabbitMqMessageReceiver).Meter("Message deserialization exception", Unit.Items);
        public static readonly Meter MeterMessageConsumeException = Metric.Context(McUfRabbitMqMessageReceiver).Meter("Message consuming exception", Unit.Items);

        /// <summary>
        /// Default context for UfSdkSportEvent
        /// </summary>
        private const string McUfSportEvent = "UfSdkSportEvent";
        public static readonly Timer TimerFetchMissingSummary = Metric.Context(McUfSportEvent).Timer("FetchMissingSummary", Unit.Items);
        public static readonly Timer TimerFetchMissingFixtures = Metric.Context(McUfSportEvent).Timer("FetchMissingFixtures", Unit.Items);
    }
}
