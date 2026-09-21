using MediatR;

namespace OrderFlow.Application.Features.Inventory.Commands.ReserveInventory;

public sealed record ReserveInventoryCommand(Guid OrderId) : IRequest;