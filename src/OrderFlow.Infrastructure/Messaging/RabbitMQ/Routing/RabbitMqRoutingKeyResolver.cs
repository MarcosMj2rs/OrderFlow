using OrderFlow.Domain.Events;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Routing;

public sealed class RabbitMqRoutingKeyResolver : IRabbitMqRoutingKeyResolver
{
    private static readonly IReadOnlyDictionary<Type, string> RoutingKeys = new Dictionary<Type, string>
    {
        [typeof(OrderCreatedDomainEvent)] = "order.created",
        [typeof(OrderCancelledDomainEvent)] = "order.cancelled",
        [typeof(OrderPaidDomainEvent)] = "order.paid"
    };

    public string Resolve(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        Type eventType = domainEvent.GetType();

        if (RoutingKeys.TryGetValue(eventType, out string? routingKey))
            return routingKey;

        throw new InvalidOperationException($"No RabbitMQ routing key was configured for domain event '{eventType.Name}'.");
    }
}
