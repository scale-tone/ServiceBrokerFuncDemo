using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;

[assembly: WebJobsStartup(typeof(ServiceBrokerTrigger.ExtensionConfigProvider))]

namespace ServiceBrokerTrigger
{
    [Extension(nameof(ServiceBrokerTrigger))]
    public class ExtensionConfigProvider : IExtensionConfigProvider, IWebJobsStartup
    {
        private readonly ILoggerFactory? loggerFactory;
        private readonly INameResolver? nameResolver;

        public ExtensionConfigProvider()
        {
        }

        public ExtensionConfigProvider(ILoggerFactory loggerFactory, INameResolver nameResolver)
        {
            this.loggerFactory = loggerFactory;
            this.nameResolver = nameResolver;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var triggerRule = context.AddBindingRule<ServiceBrokerTriggerAttribute>();
            triggerRule.BindToTrigger(new TriggerBindingProvider(this.nameResolver!, this.loggerFactory!));
        }

		public void Configure(IWebJobsBuilder builder)
		{
			builder.AddExtension<ExtensionConfigProvider>();
		}
    }
}
