using System.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus;
using Task = System.Threading.Tasks.Task;

namespace MonitoringFunctionApp.ServiceBus
{
    public static class DeadLetterMonitoring
    {
        private static string _connectionString;
        private static NamespaceManager _namespaceManager;
        private static readonly string InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];

        [FunctionName("DeadLetterCount")]
        public static async Task MonitorDeadLetterCount([TimerTrigger("0 */5 * * * *")]TimerInfo timer, TraceWriter log)
        {
            _connectionString = ConfigurationManager.AppSettings["ConnString"];

            _namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);

            var subscription = await _namespaceManager.GetSubscriptionAsync("topicPath", "topicName");

            var deadLetterMessageCount = 0L;

            deadLetterMessageCount = subscription.MessageCountDetails.DeadLetterMessageCount;
           
            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(InstrumentationKey));

            var deadLetterMetric = new MetricTelemetry("DeadLetterCount", deadLetterMessageCount);
            
            telemetryClient.TrackMetric(deadLetterMetric);
        }
    }
}
