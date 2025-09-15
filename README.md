# ServiceBrokerFuncDemo

Draft &amp; dirty implementation of an Azure Functions Trigger for [SQL Server Service Broker](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-service-broker) and a demo Function App for it.

## Contents

* **ServiceBrokerTrigger** - implements the Trigger.
* **ServiceBrokerInProcTestFunc** - a .NET InProc Function which uses that Trigger to process messages from a queue. It also sends sample messages to its own queue for demo purposes.

## How to run

0. As a prerequisite, you will need [Azure Functions Core Tools globally installed](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local#install-the-azure-functions-core-tools).

1. Enable Service Broker on your database (if not yet):
```
ALTER DATABASE MyDb SET ENABLE_BROKER;
```

2. Create a queue and a service:
```
CREATE QUEUE MyServiceBrokerQueue WITH POISON_MESSAGE_HANDLING (STATUS = OFF);
CREATE SERVICE MyServiceBrokerService ON QUEUE MyServiceBrokerQueue ([DEFAULT]);
```

3. Go to **ServiceBrokerInProcTestFunc** folder, rename `local.settings.json.sample` to `local.settings.json` and modify settings there accordingly.

4. Run `func start` in **ServiceBrokerInProcTestFunc** folder.