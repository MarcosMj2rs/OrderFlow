using OrderFlow.Domain.Events;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Routing;

public interface IRabbitMqRoutingKeyResolver
{
    string Resolve(IDomainEvent domainEvent);
}
