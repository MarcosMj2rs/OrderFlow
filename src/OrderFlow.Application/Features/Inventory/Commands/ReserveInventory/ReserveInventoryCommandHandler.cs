using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Exceptions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Inventory.Commands.ReserveInventory;

public sealed class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand>
{
    private readonly IInventoryReservationRepository _inventoryReservationRepository;
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveInventoryCommandHandler(IInventoryReservationRepository inventoryReservationRepository,
                                          IOrderSagaRepository orderSagaRepository,
                                          IOrderTransactionExecutor transactionExecutor,
                                          IUnitOfWork unitOfWork)
    {
        _inventoryReservationRepository = inventoryReservationRepository;
        _orderSagaRepository = orderSagaRepository;
        _transactionExecutor = transactionExecutor;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
    {
        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var orderSaga = await _orderSagaRepository
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (orderSaga is null)
                    throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

                // Eventos atrasados não devem criar novas reservas.
                if (orderSaga.Status is EOrderSagaStatus.COMPENSATING or EOrderSagaStatus.COMPENSATED)
                    return;

                if (orderSaga.Status != EOrderSagaStatus.PAYMENT_COMPLETED)
                    throw new DomainException($"Order saga '{request.OrderId}' cannot reserve inventory from status '{orderSaga.Status}'.");

                var existingReservation = await _inventoryReservationRepository.GetByOrderIdAsync(request.OrderId, ct);

                if (existingReservation is not null)
                    return;

                var reservation = new InventoryReservation(request.OrderId);

                await _inventoryReservationRepository.AddAsync(reservation, ct);

                try
                {
                    await _unitOfWork.SaveChangesAsync(ct);
                }
                catch (UniqueConstraintException)
                {
                    var reservationAfterConflict = await _inventoryReservationRepository.GetByOrderIdAsync(request.OrderId, ct);

                    if (reservationAfterConflict is not null)
                        return;

                    throw;
                }
            },
            cancellationToken);
    }
}