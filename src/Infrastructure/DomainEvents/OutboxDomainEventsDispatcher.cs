using System.Reflection;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Text.Json;

namespace Infrastructure.DomainEvents;

internal sealed class OutboxDomainEventsDispatcher(
    ApplicationDbContext context,
    IServiceProvider serviceProvider,
    ILogger<OutboxDomainEventsDispatcher> logger) : IDomainEventsDispatcher
{
    private static readonly Dictionary<string, Type> EventTypesCache = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToDictionary(t => t.Name, t => t);

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        string sql = """
                    SELECT id, type, content, occurred_on, processed_on, error
                    FROM public."OutboxMessages" 
                    WHERE processed_on IS NULL 
                    ORDER BY occurred_on 
                    LIMIT 20 
                    FOR UPDATE SKIP LOCKED
                    """;

        List<OutboxMessage> unprocessedMessages = await context.OutboxMessages
            .FromSqlRaw(sql)
            .ToListAsync(cancellationToken);

        foreach (OutboxMessage message in unprocessedMessages)
        {
            try
            {
                IDomainEvent? domainEvent = DeserializeDomainEvent(message);
                if (domainEvent is null)
                {
                    message.MarkAsFailed("Failed to deserialize domain event");
                    continue;
                }

                await DispatchDomainEventAsync(domainEvent, cancellationToken);

                message.MarkAsProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                message.MarkAsFailed(ex.Message);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private IDomainEvent? DeserializeDomainEvent(OutboxMessage message)
    {
        try
        {
            // 3. Быстрый поиск за O(1)
            if (!EventTypesCache.TryGetValue(message.Type, out Type? eventType))
            {
                return null;
            }

            return (IDomainEvent?)JsonSerializer.Deserialize(message.Content, eventType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing domain event {Type}", message.Type);
            return null;
        }
    }
    private async Task DispatchDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type domainEventType = domainEvent.GetType();
        Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);

        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (object? handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            MethodInfo? handleMethod = handlerType.GetMethod("Handle");
            if (handleMethod is not null)
            {
                var handleTask = handleMethod.Invoke(handler, [domainEvent, cancellationToken]) as Task;
                if (handleTask is not null)
                {
                    await handleTask;
                }
            }
        }
    }
}
