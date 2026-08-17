using OrderFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Extensions.Options;

namespace OrderFlow.Infrastructure.Persistence.Inbox;

public sealed class InboxProcessor : IInboxProcessor
{
    private readonly IInboxRepository _inboxRepository;

    private readonly OrderFlowDbContext _dbContext;

    private readonly IDbContextFactory<OrderFlowDbContext> _dbContextFactory;

    private readonly InboxOptions _options;

    private TimeSpan ProcessingTimeout => TimeSpan.FromMinutes(_options.ProcessingTimeoutMinutes);


    public InboxProcessor(IInboxRepository inboxRepository,
                          OrderFlowDbContext dbContext,
                          IDbContextFactory<OrderFlowDbContext> dbContextFactory,
                          IOptions<InboxOptions> options)
    {
        _inboxRepository = inboxRepository;
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
    }

    public async Task<EInboxProcessingResult> ProcessAsync(Guid eventId,
                                                           string type,
                                                           string payload,
                                                           Func<CancellationToken, Task> handler,
                                                           CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var existingMessage = await _inboxRepository.GetByEventIdAsync(eventId, cancellationToken);

        if (existingMessage?.Status == EInboxMessageStatus.PROCESSED)
        {
            await transaction.CommitAsync(cancellationToken);
            return EInboxProcessingResult.ALREADY_PROCESSED;
        }

        if (existingMessage?.Status == EInboxMessageStatus.PROCESSING)
        {
            DateTime utcNow = DateTime.UtcNow;

            if (!existingMessage.HasProcessingTimedOut(utcNow, ProcessingTimeout))
            {
                await transaction.CommitAsync(cancellationToken);
                return EInboxProcessingResult.ALREADY_PROCESSING;
            }

            existingMessage.RestartProcessing(utcNow);
        }

        if (existingMessage?.Status == EInboxMessageStatus.FAILED)
            existingMessage.MarkAsProcessing(DateTime.UtcNow);

        var inboxMessage = existingMessage;

        try
        {
            if (inboxMessage is null)
            {
                inboxMessage = new InboxMessage(eventId, type, payload, DateTime.UtcNow);

                await _inboxRepository.AddAsync(inboxMessage, cancellationToken);
            }

            await handler(cancellationToken);

            inboxMessage.MarkAsProcessed(DateTime.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return EInboxProcessingResult.PROCESSED;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            await PersistFailureAsync(eventId, type, payload, ex.Message, cancellationToken);

            throw;
        }
    }

    private async Task PersistFailureAsync(Guid eventId,
                                           string type,
                                           string payload,
                                           string error,
                                           CancellationToken cancellationToken)
    {
        await using OrderFlowDbContext failureContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        InboxMessage? inboxMessage = await failureContext.InboxMessages
            .SingleOrDefaultAsync(message => message.EventId == eventId, cancellationToken);

        if (inboxMessage is null)
        {
            inboxMessage = new InboxMessage(eventId, type, payload, DateTime.UtcNow);

            failureContext.InboxMessages.Add(inboxMessage);
        }

        inboxMessage.MarkAsFailed(error);

        await failureContext.SaveChangesAsync(cancellationToken);
    }
}
