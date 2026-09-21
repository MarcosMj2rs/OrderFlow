using MediatR;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.StartOrderSaga;

/// <summary>
/// Starts the saga responsible for processing an order.
/// </summary>
public sealed record StartOrderSagaCommand(Guid OrderId) : IRequest;