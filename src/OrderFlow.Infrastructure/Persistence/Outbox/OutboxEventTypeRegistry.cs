using OrderFlow.Domain.Events;

namespace OrderFlow.Infrastructure.Persistence.Outbox;

public sealed class OutboxEventTypeRegistry : IOutboxEventTypeRegistry
{
    private static readonly IReadOnlyDictionary<string, Type> EventTypes =
        new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            [nameof(OrderCreatedDomainEvent)] = typeof(OrderCreatedDomainEvent),
            [nameof(OrderCancelledDomainEvent)] = typeof(OrderCancelledDomainEvent),
            [nameof(OrderPaidDomainEvent)] = typeof(OrderPaidDomainEvent)
        };

    public Type Resolve(string eventTypeName)
    {
        if (string.IsNullOrWhiteSpace(eventTypeName))
            throw new ArgumentException("Event type name cannot be empty.", nameof(eventTypeName));

        if (!EventTypes.TryGetValue(eventTypeName, out Type? eventType))
            throw new InvalidOperationException($"Outbox event type '{eventTypeName}' is not registered.");

        if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
            throw new InvalidOperationException($"Registered type '{eventType.FullName}' does not implement {nameof(IDomainEvent)}.");

        return eventType;
    }
}