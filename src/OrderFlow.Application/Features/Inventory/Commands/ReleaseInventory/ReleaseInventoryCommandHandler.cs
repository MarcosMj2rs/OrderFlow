
using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Inventory.Commands.ReleaseInventory;

public sealed class ReleaseInventoryCommandHandler : IRequestHandler<ReleaseInventoryCommand>
{
    private readonly IInventoryReservationRepository _inventoryReservationRepository;
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IUnitOfWork _unitOfWork;

    public ReleaseInventoryCommandHandler(IInventoryReservationRepository inventoryReservationRepository,
                                          IOrderSagaRepository orderSagaRepository,
                                          IOrderTransactionExecutor transactionExecutor,
                                          IUnitOfWork unitOfWork)
    {
        _inventoryReservationRepository = inventoryReservationRepository;
        _orderSagaRepository = orderSagaRepository;
        _transactionExecutor = transactionExecutor;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReleaseInventoryCommand request, CancellationToken cancellationToken)
    {
        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var orderSaga = await _orderSagaRepository
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (orderSaga is null)
                    throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

                var reservation = await _inventoryReservationRepository.GetByOrderIdAsync(request.OrderId, ct);

                if (reservation is null)
                    throw new DomainException($"Inventory reservation for order '{request.OrderId}' was not found.");

                // Idempotência: reserva já liberada.
                if (reservation.Status == EInventoryReservationStatus.RELEASED)
                    return;

                // A liberação somente é permitida durante a compensação.
                if (orderSaga.Status != EOrderSagaStatus.COMPENSATING)
                {
                    throw new DomainException($"Order saga '{request.OrderId}' cannot release inventory from status '{orderSaga.Status}'.");
                }

                reservation.Release();

                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);
    }
}
