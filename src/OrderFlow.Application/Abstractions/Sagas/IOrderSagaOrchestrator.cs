namespace OrderFlow.Application.Abstractions.Sagas;

/// <summary>
/// Coordinates the execution of the order saga.
/// </summary>
public interface IOrderSagaOrchestrator
{
    Task StartAsync(Guid orderId, CancellationToken cancellationToken = default);
}