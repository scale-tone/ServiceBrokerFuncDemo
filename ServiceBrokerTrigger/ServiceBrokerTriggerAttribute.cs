using System;
using Microsoft.Azure.WebJobs.Description;

namespace ServiceBrokerTrigger
{
    [Binding]
    public class ServiceBrokerTriggerAttribute : Attribute
    {
        [AutoResolve]
        public string ConnectionStringName { get; set; }

        [AutoResolve]
        public string QueueName { get; set; }

        [AutoResolve]
        public string PollIntervalInMs { get; set; }

        [AutoResolve]
        public string DegreeOfParallelizm { get; set; } = "1";

        public string[] MessageTypeNames { get; set; } = new[] { "DEFAULT" };
    }
}