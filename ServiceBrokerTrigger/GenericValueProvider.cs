using System;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Bindings;

namespace ServiceBrokerTrigger
{
    public class GenericValueProvider<TValue> : IValueProvider
    {
        private readonly TValue item;

        public GenericValueProvider(TValue item)
        {
            this.item = item;
        }

        public Type Type => typeof(TValue);

        public async Task<object> GetValueAsync()
        {
            return this.item;
        }

        public string ToInvokeString()
        {
            return this.item?.ToString();
        }
    }
}

