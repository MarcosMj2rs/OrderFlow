
using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Inventory.Commands.ReserveInventory;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.HandlePaymentCompleted;

public sealed class HandlePaymentCompletedCommandHandler : IRequestHandler<HandlePaymentCompletedCommand>
{
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly IOrderTransactionExecutor _transactionExecutor;

    public HandlePaymentCompletedCommandHandler(IOrderSagaRepository orderSagaRepository,
                                                IUnitOfWork unitOfWork,
                                                ISender sender,
                                                IOrderTransactionExecutor transactionExecutor)
    {
        _orderSagaRepository = orderSagaRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _transactionExecutor = transactionExecutor;
    }

    public async Task Handle(HandlePaymentCompletedCommand request, CancellationToken cancellationToken)
    {
        var shouldReserveInventory = false;

        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var orderSaga = await _orderSagaRepository
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (orderSaga is null)
                    throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

                // Eventos atrasados não reativam uma Saga em compensação.
                if (orderSaga.Status is EOrderSagaStatus.COMPENSATING or EOrderSagaStatus.COMPENSATED)
                    return;

                if (orderSaga.Status == EOrderSagaStatus.STARTED)
                {
                    orderSaga.MarkPaymentCompleted();

                    await _unitOfWork.SaveChangesAsync(ct);
                }

                // Reprocessamento idempotente de eventos já superados.
                if (orderSaga.Status is EOrderSagaStatus.INVENTORY_RESERVED or EOrderSagaStatus.COMPLETED)
                    return;

                if (orderSaga.Status != EOrderSagaStatus.PAYMENT_COMPLETED)
                    throw new DomainException($"Order saga '{request.OrderId}' cannot process payment completion from status '{orderSaga.Status}'.");

                shouldReserveInventory = true;
            },
            cancellationToken);

        // Fora da transação: ReserveInventoryCommand usa seu próprio bloqueio.
        if (shouldReserveInventory)
            await _sender.Send(new ReserveInventoryCommand(request.OrderId), cancellationToken);
    }
}
