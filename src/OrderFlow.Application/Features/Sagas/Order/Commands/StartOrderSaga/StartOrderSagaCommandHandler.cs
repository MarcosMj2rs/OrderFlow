using MediatR;
using OrderFlow.Application.Abstractions.Sagas;

namespace OrderFlow.Application.Features.Sagas.Order.Commands.StartOrderSaga;

public sealed class StartOrderSagaCommandHandler : IRequestHandler<StartOrderSagaCommand>
{
    private readonly IOrderSagaOrchestrator _orchestrator;

    public StartOrderSagaCommandHandler(IOrderSagaOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task Handle(StartOrderSagaCommand request, CancellationToken cancellationToken)
    {
        await _orchestrator.StartAsync(request.OrderId, cancellationToken);
    }
}