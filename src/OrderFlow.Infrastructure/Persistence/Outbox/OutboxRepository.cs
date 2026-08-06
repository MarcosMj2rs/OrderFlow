using OrderFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Infrastructure.Persistence.Outbox;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly OrderFlowDbContext _context;

    public OutboxRepository(OrderFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
