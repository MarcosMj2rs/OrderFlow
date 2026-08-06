using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Infrastructure.Persistence.Context;
using OrderFlow.Infrastructure.Persistence.Outbox;

namespace OrderFlow.Infrastructure.Persistence.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderFlowDbContext _context;
    private readonly IDomainEventCollector _domainEventCollector;
    private readonly IOutboxMessageFactory _outboxMessageFactory;

    public UnitOfWork(OrderFlowDbContext context,
                      IDomainEventCollector domainEventCollector,
                      IOutboxMessageFactory outboxMessageFactory)
    {
        _context = context;
        _domainEventCollector = domainEventCollector;
        _outboxMessageFactory = outboxMessageFactory;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DomainEventCollection domainEventCollection = _domainEventCollector.Collect();

        OutboxMessage[] outboxMessages = domainEventCollection.Events
                .Select(_outboxMessageFactory.Create)
                .ToArray();

        if (outboxMessages.Length > 0)
            await _context.OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        Clear(domainEventCollection.Entities);
    }

    private static void Clear(IReadOnlyCollection<Entity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (Entity entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}