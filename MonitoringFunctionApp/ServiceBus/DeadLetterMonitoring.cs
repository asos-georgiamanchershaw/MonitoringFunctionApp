using System.Configuration;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus;

namespace MonitoringFunctionApp.ServiceBus
{
    public static class DeadLetterMonitoring
    {
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["ConnString"];
        private static readonly string InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
        private static NamespaceManager _namespaceManager;

        [FunctionName("DeadLetterCount")]
        public static async Task MonitorDeadLetterCount([TimerTrigger("0 */5 * * * *")]TimerInfo timer, TraceWriter log)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

            var subscription = await _namespaceManager.GetSubscriptionAsync("topicPath", "topicName");

            var deadLetterMessageCount = 0L;

            deadLetterMessageCount = subscription.MessageCountDetails.DeadLetterMessageCount;
           
            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(InstrumentationKey));

            var deadLetterMetric = new MetricTelemetry("DeadLetterCount", deadLetterMessageCount);
            
            telemetryClient.TrackMetric(deadLetterMetric);
        }
    }
}
