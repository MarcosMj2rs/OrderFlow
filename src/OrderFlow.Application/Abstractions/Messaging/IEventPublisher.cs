using OrderFlow.Domain.Events;

namespace OrderFlow.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
