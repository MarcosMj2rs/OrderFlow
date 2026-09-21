using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Commands.PayOrder;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.HandleInventoryReserved;

public sealed class HandleInventoryReservedCommandHandler : IRequestHandler<HandleInventoryReservedCommand>
{
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public HandleInventoryReservedCommandHandler(IOrderSagaRepository orderSagaRepository, IUnitOfWork unitOfWork, ISender sender)
    {
        _orderSagaRepository = orderSagaRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task Handle(HandleInventoryReservedCommand request, CancellationToken cancellationToken)
    {
        var orderSaga = await _orderSagaRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (orderSaga is null)
            throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

        orderSaga.MarkInventoryReserved();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _sender.Send(new PayOrderCommand(request.OrderId), cancellationToken);

        orderSaga.Complete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}