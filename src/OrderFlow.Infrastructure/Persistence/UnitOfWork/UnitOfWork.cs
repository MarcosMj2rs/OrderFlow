using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderFlowDbContext _context;
    private readonly IDomainEventCollector _domainEventCollector;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(OrderFlowDbContext context, IDomainEventCollector domainEventCollector, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventCollector = domainEventCollector;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DomainEventCollection domainEventCollection = _domainEventCollector.Collect();

        await _context.SaveChangesAsync(cancellationToken);
        await _domainEventDispatcher.DispatchAsync(domainEventCollection.Events, cancellationToken);

        Clear(domainEventCollection.Entities);
    }

    private static void Clear(IReadOnlyCollection<Entity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (Entity entity in entities)
            entity.ClearDomainEvents();
    }
}