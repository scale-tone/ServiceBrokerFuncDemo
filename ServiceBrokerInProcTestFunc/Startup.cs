using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Data.SqlClient;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Azure.Monitor.OpenTelemetry.Exporter;
using System.Diagnostics.Metrics;


[assembly: WebJobsStartup(typeof(ServiceBrokerInProcTestFunc.Startup))]

namespace ServiceBrokerInProcTestFunc
{
    public class Startup : IWebJobsStartup
    {
        internal static readonly Meter Meter = new Meter(nameof(ServiceBrokerInProcTestFunc));
        internal static readonly Counter<int> SentCounter = Meter.CreateCounter<int>("msg.sent.count");
        internal static readonly Counter<int> SendFailedCounter = Meter.CreateCounter<int>("msg.sendfailed.count");
        internal static readonly Histogram<int> LatencyCounter = Meter.CreateHistogram<int>("msg.delivery.latency");

        public void Configure(IWebJobsBuilder builder)
        {
            string appInsightsConnString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(appInsightsConnString))
            {
                Sdk.CreateMeterProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault())
                    .AddMeter(nameof(ServiceBrokerInProcTestFunc))
                    .AddAzureMonitorMetricExporter(o => o.ConnectionString = appInsightsConnString)
                    .Build();
            }

            SendMessagesAsync();
        }

        private async Task SendMessagesAsync()
        {
            int messagesPerSecond = int.Parse(Environment.GetEnvironmentVariable("MESSAGES_PER_SECOND"));

            while (true)
            {
                for(int i = 0; i < messagesPerSecond; i++)
                {
                    this.SendMessageAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task SendMessageAsync()
        {
            try
            {
                string connString = Environment.GetEnvironmentVariable("SQL_CONN_STRING");
                string serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME");

                string sql = @$"DECLARE @dlg AS UNIQUEIDENTIFIER;
                    BEGIN DIALOG @dlg FROM SERVICE {serviceName} TO SERVICE N'{serviceName}' WITH ENCRYPTION = OFF;
                    SEND ON CONVERSATION (@dlg) (N'{DateTimeOffset.Now:O}');
                END CONVERSATION @dlg;";

                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    SentCounter.Add(1);
                }
            }
            catch (Exception ex)
            {
                SendFailedCounter.Add(1);
                Console.WriteLine($"Failed to send message. {ex}");
            }
        }
    }
}
