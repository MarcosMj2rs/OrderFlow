
using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Commands.PayOrder;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.HandleInventoryReserved;

public sealed class HandleInventoryReservedCommandHandler
    : IRequestHandler<HandleInventoryReservedCommand>
{
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public HandleInventoryReservedCommandHandler(IOrderSagaRepository orderSagaRepository,
                                                 IOrderRepository orderRepository,
                                                 IOrderTransactionExecutor transactionExecutor,
                                                 IUnitOfWork unitOfWork,
                                                 ISender sender)
    {
        _orderSagaRepository = orderSagaRepository;
        _orderRepository = orderRepository;
        _transactionExecutor = transactionExecutor;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task Handle(HandleInventoryReservedCommand request, CancellationToken cancellationToken)
    {
        var shouldPayOrder = false;

        // Fase 1: avançar a Saga sob bloqueio transacional.
        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var saga = await _orderSagaRepository
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (saga is null)
                    throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

                if (saga.Status is
                    EOrderSagaStatus.COMPENSATING or
                    EOrderSagaStatus.COMPENSATED or
                    EOrderSagaStatus.COMPLETED)
                {
                    return;
                }

                if (saga.Status == EOrderSagaStatus.PAYMENT_COMPLETED)
                {
                    saga.MarkInventoryReserved();
                    await _unitOfWork.SaveChangesAsync(ct);
                }

                if (saga.Status != EOrderSagaStatus.INVENTORY_RESERVED)
                    throw new DomainException($"Order saga '{request.OrderId}' cannot process inventory reservation from status '{saga.Status}'.");

                shouldPayOrder = true;
            },
            cancellationToken);

        if (!shouldPayOrder)
            return;

        // Fase 2: PayOrderCommand possui seu próprio bloqueio.
        await _sender.Send(new PayOrderCommand(request.OrderId), cancellationToken);
    }
}
