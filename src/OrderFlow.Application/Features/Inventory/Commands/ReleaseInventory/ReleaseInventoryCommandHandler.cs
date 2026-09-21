using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Inventory.Commands.ReleaseInventory;

public sealed class ReleaseInventoryCommandHandler : IRequestHandler<ReleaseInventoryCommand>
{
    private readonly IInventoryReservationRepository _inventoryReservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReleaseInventoryCommandHandler(IInventoryReservationRepository inventoryReservationRepository, IUnitOfWork unitOfWork)
    {
        _inventoryReservationRepository = inventoryReservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReleaseInventoryCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _inventoryReservationRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (reservation is null)
            throw new DomainException($"Inventory reservation for order '{request.OrderId}' was not found.");

        reservation.Release();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}