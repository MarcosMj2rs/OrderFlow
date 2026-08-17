using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Inbox;

public sealed class InboxProcessor : IInboxProcessor
{
    private readonly IInboxRepository _inboxRepository;
    private readonly OrderFlowDbContext _dbContext;

    public InboxProcessor(IInboxRepository inboxRepository, OrderFlowDbContext dbContext)
    {
        _inboxRepository = inboxRepository;
        _dbContext = dbContext;
    }

    public async Task<bool> ProcessAsync(Guid eventId,
                                         string type,
                                         string payload,
                                         Func<CancellationToken, Task> handler,
                                         CancellationToken cancellationToken = default)
    {
        var existingMessage = await _inboxRepository.GetByEventIdAsync(eventId, cancellationToken);

        if (existingMessage?.ProcessedOnUtc is not null)
            return false;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var inboxMessage = existingMessage;

            if (inboxMessage is null)
            {
                inboxMessage = new InboxMessage
                (
                    eventId,
                    type,
                    payload,
                    DateTime.UtcNow
                );

                await _inboxRepository.AddAsync(inboxMessage, cancellationToken);
            }

            await handler(cancellationToken);

            inboxMessage.MarkAsProcessed(DateTime.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
