using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Inbox;

public sealed class InboxRepository : IInboxRepository
{
    private readonly OrderFlowDbContext _context;

    public InboxRepository(OrderFlowDbContext context)
    {
        _context = context;
    }

    public async Task<InboxMessage?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await _context.InboxMessages.SingleOrDefaultAsync(message => message.EventId == eventId, cancellationToken);
    }

    public async Task AddAsync(InboxMessage inboxMessage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inboxMessage, nameof(inboxMessage));
        await _context.InboxMessages.AddAsync(inboxMessage, cancellationToken);
    }
}
