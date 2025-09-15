using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ServiceBrokerTrigger
{
    public class ServiceBrokerQueueListener : IListener
    {
        private readonly ITriggeredFunctionExecutor executor;
        private readonly ServiceBrokerTriggerAttribute triggerAttribute;
        private readonly ILogger<ServiceBrokerQueueListener> log;
        private CancellationTokenSource? cts;

        public ServiceBrokerQueueListener(ITriggeredFunctionExecutor executor, ServiceBrokerTriggerAttribute attr, ILoggerFactory loggerFactory)
        {
            this.executor = executor;
            this.triggerAttribute = attr;

            this.log = loggerFactory.CreateLogger<ServiceBrokerQueueListener>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.cts = new CancellationTokenSource();
            var ct = cts.Token;

            await Task.WhenAll(
                Enumerable.Range(0, int.Parse(this.triggerAttribute.DegreeOfParallelizm))
                .Select(i => this.ReceiveMessagesAsync(ct)));
        }

        public void Cancel()
        {
            this.cts?.Cancel();
        }

        public void Dispose()
        {
            this.Cancel();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.Cancel();
        }

        private async Task ReceiveMessagesAsync(CancellationToken ct)
        {
            string connString = Environment.GetEnvironmentVariable(this.triggerAttribute.ConnectionStringName);
            string queueName = this.triggerAttribute.QueueName;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var messages = new List<string>();

                    string sql = @$"WAITFOR(
                        RECEIVE message_type_name, CAST(message_body AS nvarchar(max)) AS msg FROM {queueName}
                    ), TIMEOUT {this.triggerAttribute.PollIntervalInMs}";

                    using (var conn = new SqlConnection(connString))
                    {
                        conn.Open();

                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync(ct))
                            {
                                while (await reader.ReadAsync(ct))
                                {
                                    string msgTypeName = reader["message_type_name"].ToString();
                                    if (this.triggerAttribute.MessageTypeNames.Contains(msgTypeName))
                                    {
                                        messages.Add(reader["msg"].ToString());
                                    }
                                }
                            }
                        }

                        // Processing needs to be a part of transaction
                        if (messages.Any())
                        {
                            var data = new TriggeredFunctionData { TriggerValue = messages.ToArray() };

                            await this.executor.TryExecuteAsync(data, ct);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"Failed to receive messages from {queueName}", ex);
                    Console.WriteLine($"ERROR: failed to receive messages from {queueName}. {ex.Message}");
                }
            }
        }
    }
}