using OrderFlow.Domain.Events;

namespace OrderFlow.Infrastructure.Persistence.Outbox;

public interface IOutboxMessageFactory
{
    OutboxMessage Create(IDomainEvent domainEvent);
}
