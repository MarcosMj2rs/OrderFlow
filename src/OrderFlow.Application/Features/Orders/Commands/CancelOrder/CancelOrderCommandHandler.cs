
using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Abstractions.Sagas;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IOrderSagaOrchestrator _orderSagaOrchestrator;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IOrderRepository orderRepository,
                                     IOrderSagaRepository orderSagaRepository,
                                     IOrderSagaOrchestrator orderSagaOrchestrator,
                                     IOrderTransactionExecutor transactionExecutor,
                                     IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _orderSagaRepository = orderSagaRepository;
        _orderSagaOrchestrator = orderSagaOrchestrator;
        _transactionExecutor = transactionExecutor;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var hasSaga = false;

        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var order = await _orderRepository
                    .GetByIdAsync(request.OrderId, ct);

                if (order is null)
                    throw new DomainException("Order not found.");

                var saga = await _orderSagaRepository.GetByOrderIdAsync(request.OrderId, ct);

                if (saga is not null)
                {
                    // A compensação será coordenada pelo orquestrador.
                    hasSaga = true;
                    return;
                }

                // Pedido sem Saga: cancelamento simples.
                if (order.Status == OrderStatus.CANCELLED)
                    return;

                if (order.Status != OrderStatus.PENDING)
                {
                    throw new DomainException($"Order '{request.OrderId}' cannot be cancelled from status '{order.Status}'.");
                }

                order.Cancel();

                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);

        // Fora da transação para evitar bloqueio aninhado.
        if (hasSaga)
        {
            await _orderSagaOrchestrator.CancelAsync(request.OrderId, cancellationToken);
        }
    }
}
