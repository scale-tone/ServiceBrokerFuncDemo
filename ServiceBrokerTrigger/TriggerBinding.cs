using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace ServiceBrokerTrigger
{
    public class TriggerBinding : ITriggerBinding
    {
        private readonly ServiceBrokerTriggerAttribute triggerAttribute;
        private readonly ILoggerFactory loggerFactory;
        

        public TriggerBinding(ServiceBrokerTriggerAttribute attr, ILoggerFactory loggerFactory)
        {
            this.triggerAttribute = attr;
            this.loggerFactory = loggerFactory;
        }

        public Type TriggerValueType => typeof(string[]);

        public async Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            // Some plumming between this binding and triggered method's parameter
            return new TriggerData
            (
                new GenericValueProvider<string[]>((string[])value),
                new Dictionary<string, object>()
            );
        }

        public async Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return new ServiceBrokerQueueListener(context.Executor, this.triggerAttribute, this.loggerFactory);
        }

        /// <summary>
        /// TODO: figure out what this property is for and when it is used
        /// </summary>
        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        /// <summary>
        /// TODO: figure out what this method is for and when it is used
        /// </summary>
        public ParameterDescriptor ToParameterDescriptor()
        {
            return new TriggerParameterDescriptor()
            {
                Name = "ServiceBrokerTrigger"
            };
        }
    }
}