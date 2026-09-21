using MediatR;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.HandlePaymentCompleted;

/// <summary>
/// Handles the successful completion of the payment step of an order saga.
/// </summary>
public sealed record HandlePaymentCompletedCommand(Guid OrderId, Guid PaymentId) : IRequest;