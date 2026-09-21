using MediatR;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.HandleInventoryReserved;

/// <summary>
/// Handles the successful reservation of inventory for an order saga.
/// </summary>
public sealed record HandleInventoryReservedCommand(Guid OrderId, Guid ReservationId) : IRequest;