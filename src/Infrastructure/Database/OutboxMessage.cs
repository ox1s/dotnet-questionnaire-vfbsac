using SharedKernel;

namespace Infrastructure.Database;

public sealed class OutboxMessage : Entity
{
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    private OutboxMessage(Guid id, string type, string content, DateTime occurredOn) : base(id)
    {
        Type = type;
        Content = content;
        OccurredOn = occurredOn;
    }

    public static OutboxMessage Create(IDomainEvent domainEvent)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            domainEvent.GetType().Name,
            System.Text.Json.JsonSerializer.Serialize(domainEvent),
            DateTime.UtcNow);
    }

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}
