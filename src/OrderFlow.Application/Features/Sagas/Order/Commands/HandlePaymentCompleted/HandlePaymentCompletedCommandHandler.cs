using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Inventory.Commands.ReserveInventory;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.HandlePaymentCompleted;

public sealed class HandlePaymentCompletedCommandHandler : IRequestHandler<HandlePaymentCompletedCommand>
{
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public HandlePaymentCompletedCommandHandler(IOrderSagaRepository orderSagaRepository, IUnitOfWork unitOfWork, ISender sender)
    {
        _orderSagaRepository = orderSagaRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task Handle(HandlePaymentCompletedCommand request, CancellationToken cancellationToken)
    {
        var orderSaga = await _orderSagaRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (orderSaga is null)
            throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

        orderSaga.MarkPaymentCompleted();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _sender.Send(new ReserveInventoryCommand(request.OrderId), cancellationToken);
    }
}