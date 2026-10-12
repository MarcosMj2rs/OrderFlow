using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Exceptions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Inventory.Commands.ReserveInventory;

public sealed class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand>
{
    private readonly IInventoryReservationRepository _inventoryReservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveInventoryCommandHandler(IInventoryReservationRepository inventoryReservationRepository, IUnitOfWork unitOfWork)
    {
        _inventoryReservationRepository = inventoryReservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
    {
        var existingReservation = await _inventoryReservationRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (existingReservation is not null)
            return;

        var reservation = new InventoryReservation(request.OrderId);

        await _inventoryReservationRepository.AddAsync(reservation, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            var reservationAfterConflict = await _inventoryReservationRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (reservationAfterConflict is not null)
                return;

            throw;
        }
    }
}