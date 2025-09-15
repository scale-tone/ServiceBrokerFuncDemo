using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace ServiceBrokerTrigger
{
    public class TriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly INameResolver nameResolver;

        public TriggerBindingProvider(INameResolver nameResolver, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.nameResolver = nameResolver;
        }

        public async Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var triggerAttribute = context.Parameter.GetCustomAttribute<ServiceBrokerTriggerAttribute>(false);
            if (triggerAttribute == null)
            {
                return null!;
            }

            triggerAttribute.QueueName = this.nameResolver.ResolveWholeString(triggerAttribute.QueueName);
            triggerAttribute.PollIntervalInMs = this.nameResolver.ResolveWholeString(triggerAttribute.PollIntervalInMs);
            triggerAttribute.DegreeOfParallelizm = this.nameResolver.ResolveWholeString(triggerAttribute.DegreeOfParallelizm);

            return new TriggerBinding(triggerAttribute, this.loggerFactory);
        }
    }
}
