namespace OrderFlow.Infrastructure.Persistence.Inbox;

public interface IInboxProcessor
{
    Task<EInboxProcessingResult> ProcessAsync(Guid eventId,
                                              string type,
                                              string payload,
                                              Func<CancellationToken, Task> handler,
                                              CancellationToken cancellationToken = default);
}
