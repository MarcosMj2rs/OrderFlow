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
        EInboxProcessingResult? startResult = await TryStartProcessingAsync(eventId,
                                                                            type,
                                                                            payload,
                                                                            cancellationToken);

        if (startResult.HasValue)
            return startResult.Value;

        try
        {
            await handler(cancellationToken);

            var inboxMessage = await _inboxRepository.GetByEventIdAsync(eventId, cancellationToken);

            if (inboxMessage is null)
                throw new InvalidOperationException($"Inbox message '{eventId}' was not found after processing started.");

            inboxMessage.MarkAsProcessed(DateTime.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return EInboxProcessingResult.PROCESSED;
        }
        catch (Exception ex)
        {
            await PersistFailureAsync(eventId,
                                      type,
                                      payload,
                                      ex.Message,
                                      cancellationToken);

            throw;
        }
    }

    private async Task<EInboxProcessingResult?> TryStartProcessingAsync(Guid eventId,
                                                                        string type,
                                                                        string payload,
                                                                        CancellationToken cancellationToken)
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

        if (existingMessage is null)
        {
            existingMessage = new InboxMessage(eventId, type, payload, DateTime.UtcNow);
            await _inboxRepository.AddAsync(existingMessage, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return null;
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
