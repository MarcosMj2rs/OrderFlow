using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Abstractions.Sagas;
using OrderFlow.Application.Features.Payments.Commands.ProcessPayment;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

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

    public OrderSagaOrchestrator(IOrderSagaRepository orderSagaRepository, IUnitOfWork unitOfWork, IOrderRepository orderRepository, ISender sender)
    {
        _orderSagaRepository = orderSagaRepository;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _sender = sender;
    }

    public async Task StartAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var existingSaga = await _orderSagaRepository.GetByOrderIdAsync(orderId, cancellationToken);

        if (existingSaga is not null)
            return;

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            throw new DomainException($"Order '{orderId}' was not found.");

        var orderSaga = new OrderSaga(orderId);

        await _orderSagaRepository.AddAsync(orderSaga, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _sender.Send(new ProcessPaymentCommand(order.Id, order.TotalAmount), cancellationToken);
    }
}