using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Events;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.DomainEvents;

public sealed class EfCoreDomainEventCollector : IDomainEventCollector
{
    private readonly OrderFlowDbContext _context;

    public EfCoreDomainEventCollector(OrderFlowDbContext dbContext)
    {
        _context = dbContext;
    }

    public DomainEventCollection Collect()
    {
        Entity[] entitiesWithDomainEvents = _context.ChangeTracker
                                                    .Entries<Entity>()
                                                    .Select(entry => entry.Entity)
                                                    .Where(entity => entity.DomainEvents.Count > 0)
                                                    .ToArray();

        IDomainEvent[] domainEvents = entitiesWithDomainEvents.SelectMany(entity => entity.DomainEvents)
                                                              .ToArray();

        return new DomainEventCollection(entitiesWithDomainEvents, domainEvents);
    }
}