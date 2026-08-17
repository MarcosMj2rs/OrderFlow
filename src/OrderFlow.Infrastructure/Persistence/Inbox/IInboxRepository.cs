namespace OrderFlow.Infrastructure.Persistence.Inbox;

public interface IInboxRepository
{
    Task<InboxMessage?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddAsync(InboxMessage inboxMessage, CancellationToken cancellationToken);
}
