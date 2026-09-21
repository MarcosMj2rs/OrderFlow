using MediatR;

namespace OrderFlow.Application.Features.Inventory.Commands.ReleaseInventory;

public sealed record ReleaseInventoryCommand(Guid OrderId) : IRequest;