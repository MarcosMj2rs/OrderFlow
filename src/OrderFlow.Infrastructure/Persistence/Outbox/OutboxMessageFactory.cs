using OrderFlow.Domain.Events;
using System.Text.Json;

namespace OrderFlow.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public OutboxMessage Create(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        string type = domainEvent.GetType().Name;

        string payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions);

        return new OutboxMessage
        (
            id: domainEvent.EventId,
            type: type,
            payload: payload,
            occurredOnUtc: domainEvent.OccurredAt
        );
    }
}
