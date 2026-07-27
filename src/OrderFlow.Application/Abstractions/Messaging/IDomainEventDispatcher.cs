using OrderFlow.Domain.Events;

namespace OrderFlow.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
