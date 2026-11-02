using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Abstractions.Sagas;
using OrderFlow.Application.Features.Payments.Commands.ProcessPayment;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Application.Features.Payments.Commands.RefundPayment;
using OrderFlow.Application.Features.Inventory.Commands.ReleaseInventory;

namespace OrderFlow.Application.Sagas.Order;

/// <summary>
/// Coordinates the execution of the order saga.
/// </summary>
public sealed class OrderSagaOrchestrator : IOrderSagaOrchestrator
{
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ISender _sender;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInventoryReservationRepository _inventoryReservationRepository;

    public OrderSagaOrchestrator(IOrderSagaRepository orderSagaRepository,
                                 IUnitOfWork unitOfWork,
                                 IOrderRepository orderRepository,
                                 ISender sender,
                                 IOrderTransactionExecutor transactionExecutor,
                                 IPaymentRepository paymentRepository,
                                 IInventoryReservationRepository inventoryReservationRepository)
    {
        _orderSagaRepository = orderSagaRepository;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _sender = sender;
        _transactionExecutor = transactionExecutor;
        _paymentRepository = paymentRepository;
        _inventoryReservationRepository = inventoryReservationRepository;
    }

    public async Task StartAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await _transactionExecutor.ExecuteAsync(
            orderId,
            async ct =>
            {
                var order = await _orderRepository.GetByIdAsync(orderId, ct);

                if (order is null)
                    throw new DomainException($"Order '{orderId}' was not found.");

                if (order.Status != OrderStatus.PENDING)
                    throw new DomainException($"Order '{orderId}' cannot start a saga from status '{order.Status}'.");

                var existingSaga = await _orderSagaRepository.GetByOrderIdAsync(orderId, ct);

                if (existingSaga is not null)
                    return;

                var orderSaga = new OrderSaga(orderId);

                await _orderSagaRepository.AddAsync(orderSaga, ct);

                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);

        // Executar fora da transação para evitar bloqueios aninhados.
        var currentSaga = await _orderSagaRepository.GetByOrderIdAsync(
            orderId,
            cancellationToken);

        if (currentSaga is null || currentSaga.Status != EOrderSagaStatus.STARTED)
            return;

        var currentOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (currentOrder is null)
            throw new DomainException($"Order '{orderId}' was not found.");

        await _sender.Send(new ProcessPaymentCommand(currentOrder.Id, currentOrder.TotalAmount), cancellationToken);
    }


    public async Task CancelAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // Fase 1: cancelar o pedido e iniciar a compensação.
        await _transactionExecutor.ExecuteAsync(
            orderId,
            async ct =>
            {
                var order = await _orderRepository.GetByIdAsync(orderId, ct);

                if (order is null)
                    throw new DomainException($"Order '{orderId}' was not found.");

                var saga = await _orderSagaRepository.GetByOrderIdAsync(orderId, ct);

                if (saga is null)
                    throw new DomainException($"Order saga for order '{orderId}' was not found.");

                if (saga.Status == EOrderSagaStatus.COMPENSATED)
                    return;

                if (saga.Status == EOrderSagaStatus.COMPLETED || order.Status == OrderStatus.PAID)
                {
                    throw new DomainException($"Order '{orderId}' cannot be cancelled after completion.");
                }

                if (saga.Status != EOrderSagaStatus.COMPENSATING)
                    saga.StartCompensation();

                if (order.Status == OrderStatus.PENDING)
                    order.Cancel();

                if (order.Status != OrderStatus.CANCELLED)
                    throw new DomainException($"Order '{orderId}' is not cancelled.");

                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);

        // Fase 2: executar as compensações fora da transação inicial.
        var sagaAfterCancellation = await _orderSagaRepository.GetByOrderIdAsync(orderId, cancellationToken);

        if (sagaAfterCancellation is null)
            throw new DomainException($"Order saga for order '{orderId}' was not found.");

        if (sagaAfterCancellation.Status == EOrderSagaStatus.COMPENSATED)
            return;

        if (sagaAfterCancellation.Status != EOrderSagaStatus.COMPENSATING)
            throw new DomainException($"Order saga '{orderId}' is not compensating.");

        var reservation = await _inventoryReservationRepository.GetByOrderIdAsync(orderId, cancellationToken);

        if (reservation is not null && reservation.Status == EInventoryReservationStatus.RESERVED)
            await _sender.Send(new ReleaseInventoryCommand(orderId), cancellationToken);

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);

        if (payment is not null && payment.Status == EPaymentStatus.COMPLETED)
            await _sender.Send(new RefundPaymentCommand(orderId), cancellationToken);

        // Fase 3: verificar as compensações e concluir a Saga.
        await _transactionExecutor.ExecuteAsync(
            orderId,
            async ct =>
            {
                var saga = await _orderSagaRepository
                    .GetByOrderIdAsync(orderId, ct);

                if (saga is null)
                    throw new DomainException($"Order saga for order '{orderId}' was not found.");

                if (saga.Status == EOrderSagaStatus.COMPENSATED)
                    return;

                if (saga.Status != EOrderSagaStatus.COMPENSATING)
                    throw new DomainException($"Order saga '{orderId}' cannot complete compensation from status '{saga.Status}'.");

                var order = await _orderRepository.GetByIdAsync(orderId, ct);

                if (order is null || order.Status != OrderStatus.CANCELLED)
                    throw new DomainException($"Order '{orderId}' must be cancelled before compensation completes.");

                var currentReservation = await _inventoryReservationRepository
                    .GetByOrderIdAsync(orderId, ct);

                if (currentReservation is not null && currentReservation.Status != EInventoryReservationStatus.RELEASED)
                {
                    throw new DomainException($"Inventory for order '{orderId}' has not been released.");
                }

                var currentPayment = await _paymentRepository.GetByOrderIdAsync(orderId, ct);

                if (currentPayment is not null && currentPayment.Status != EPaymentStatus.REFUNDED)
                    throw new DomainException($"Payment for order '{orderId}' has not been refunded.");

                saga.MarkCompensated();

                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);
    }

}