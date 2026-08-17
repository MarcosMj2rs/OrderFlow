namespace OrderFlow.Infrastructure.Persistence.Inbox;

internal interface IInboxProcessor
{
    Task<bool> ProcessAsync(Guid eventId,
                            string type,
                            string payload,
                            Func<CancellationToken, Task> handler,
                            CancellationToken cancellationToken = default);
}
