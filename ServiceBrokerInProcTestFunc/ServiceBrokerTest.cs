using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServiceBrokerTrigger;

namespace ServiceBrokerInProcTestFunc
{
    public static class ServiceBrokerTest
    {

        [FunctionName(nameof(ServiceBrokerTest))]
        public static async Task Func(
            [ServiceBrokerTrigger(
                ConnectionStringName = "SQL_CONN_STRING",
                QueueName = "%QUEUE_NAME%",
                PollIntervalInMs = "%POLL_INTERVAL_IN_MS%",
                DegreeOfParallelizm = "%DEGREE_OF_PARALLELIZM%"
            )] string[] msgs,
            ILogger log)
        {
            string msg = msgs[0];
            var dtSent = DateTimeOffset.Parse(msg);
            var latency = DateTimeOffset.Now - dtSent;

            Startup.LatencyCounter.Record((int)latency.TotalMilliseconds);

            log.LogInformation($">>> Received in: {latency.TotalMilliseconds} ms");
        }
    }
}
