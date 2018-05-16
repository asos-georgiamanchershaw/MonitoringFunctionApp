using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus;

namespace MonitoringFunctionApp.ServiceBus
{
    public static class DeadLetterMonitoring
    {
        private static readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        private static readonly string InstrumentationKey = "adf0beae-a23e-45a3-b5c1-2ad9cbedfca4";
        private static NamespaceManager _namespaceManager;

        [FunctionName("dead-letter-count")]
        public static async Task MonitorDeadLetterCount([TimerTrigger("0 */5 * * * *")]TimerInfo timer)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);

            var deadLetterMessageCount = 0L;

            var topics = await _namespaceManager.GetTopicsAsync();

            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(InstrumentationKey));

            var customEvent = new EventTelemetry("Subscription Dead Letter Count");

            foreach (var topic in topics)
            {
                if (topic.Path.Contains("warehouse-orders"))
                {
                    var subscriptions = await _namespaceManager.GetSubscriptionsAsync(topic.Path);
                    
                    foreach (var subscription in subscriptions)
                    {
                        deadLetterMessageCount = subscription.MessageCountDetails.DeadLetterMessageCount;
                        
                        customEvent.Metrics.Add(subscription.Name, deadLetterMessageCount);

                    }
                }
            }

            telemetryClient.TrackEvent(customEvent);
            

            //var telemetry  = new TraceTelemetry(subscription.Name);
            //telemetry.Properties.Add("Dead Letter Count", deadLetterMessageCount.ToString());

           //var deadLetterMetric = new MetricTelemetry("Dead Letter Count", deadLetterMessageCount);
           //telemetryClient.TrackMetric(deadLetterMetric);
        }
    }
}
