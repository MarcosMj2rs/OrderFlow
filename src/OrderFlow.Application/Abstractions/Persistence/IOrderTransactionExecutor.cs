
namespace OrderFlow.Application.Abstractions.Persistence;

/// <summary>
/// Executes operations under an exclusive,
/// transaction-scoped lock for a specific order.
/// </summary>
public interface IOrderTransactionExecutor
{
    /// <summary>
    /// Legacy contract. Preserved temporarily
    /// during the migration of existing handlers.
    /// </summary>
    Task ExecuteAsync(Guid orderId, Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation using repositories and
    /// the unit of work from an isolated transactional scope.
    /// </summary>
    Task ExecuteAsync(Guid orderId, Func<IOrderTransactionContext, CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
