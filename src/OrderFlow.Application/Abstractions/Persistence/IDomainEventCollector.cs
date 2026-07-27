using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Events;

namespace OrderFlow.Application.Abstractions.Persistence;

public interface IDomainEventCollector
{
    DomainEventCollection Collect();
}

public sealed record DomainEventCollection(IReadOnlyCollection<Entity> Entities, IReadOnlyCollection<IDomainEvent> Events);
