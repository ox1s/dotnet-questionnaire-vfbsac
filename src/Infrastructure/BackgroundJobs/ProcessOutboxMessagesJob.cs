using Infrastructure.DomainEvents;
using Quartz;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxMessagesJob(
    IDomainEventsDispatcher domainEventsDispatcher,
    ILogger<ProcessOutboxMessagesJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Processing outbox messages");

        if (domainEventsDispatcher is OutboxDomainEventsDispatcher outboxDispatcher)
        {
            await outboxDispatcher.ProcessOutboxMessagesAsync(context.CancellationToken);
        }
    }
}
