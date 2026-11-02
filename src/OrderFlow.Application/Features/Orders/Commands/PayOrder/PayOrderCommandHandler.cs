
using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Application.Features.Orders.Commands.PayOrder;

public sealed class PayOrderCommandHandler : IRequestHandler<PayOrderCommand>
{
    private readonly IOrderTransactionExecutor _transactionExecutor;

    public PayOrderCommandHandler(
        IOrderTransactionExecutor transactionExecutor)
    {
        _transactionExecutor = transactionExecutor;
    }

    public async Task Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async (transactionContext, ct) =>
            {
                var orderSaga = await transactionContext.OrderSagas
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (orderSaga is null)
                {
                    throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");
                }

                // Não reativar Sagas em compensação.
                if (orderSaga.Status is
                    EOrderSagaStatus.COMPENSATING or
                    EOrderSagaStatus.COMPENSATED)
                {
                    return;
                }

                var order = await transactionContext.Orders.GetByIdAsync(request.OrderId, ct);

                if (order is null)
                    throw new DomainException("Order not found.");

                // Reprocessamento de uma Saga já concluída.
                if (orderSaga.Status == EOrderSagaStatus.COMPLETED)
                {
                    if (order.Status != OrderStatus.PAID)
                        throw new DomainException($"Order '{request.OrderId}' is not paid, but its saga is completed.");

                    return;
                }

                if (orderSaga.Status != EOrderSagaStatus.INVENTORY_RESERVED)
                    throw new DomainException($"Order saga '{request.OrderId}' cannot pay order from status '{orderSaga.Status}'.");

                if (order.Status == OrderStatus.CANCELLED)
                    throw new DomainException($"Cancelled order '{request.OrderId}' cannot be paid.");

                // Recuperação de estados legados:
                // pedido PAID com Saga ainda INVENTORY_RESERVED.
                if (order.Status == OrderStatus.PENDING)
                    order.Pay();

                orderSaga.Complete();

                // Pedido, Saga e Outbox na mesma transação.
                await transactionContext.UnitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);
    }
}
