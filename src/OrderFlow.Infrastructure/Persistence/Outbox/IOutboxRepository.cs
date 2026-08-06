namespace OrderFlow.Infrastructure.Persistence.Outbox;

public interface IOutboxRepository
{
    Task<IReadOnlyCollection<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
