using OrderFlow.Worker.Payments.Consumers;

namespace OrderFlow.Worker.Payments.HostedServices;

public sealed class InventoryReservedConsumerHostedService : IHostedService
{
    private readonly InventoryReservedConsumer _consumer;
    private readonly ILogger<InventoryReservedConsumerHostedService> _logger;

    public InventoryReservedConsumerHostedService(InventoryReservedConsumer consumer,
                                                  ILogger<InventoryReservedConsumerHostedService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting InventoryReserved consumer hosted service...");

        await _consumer.StartAsync(cancellationToken);

        _logger.LogInformation("InventoryReserved consumer hosted service started successfully.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping InventoryReserved consumer hosted service...");

        await _consumer.DisposeAsync();

        _logger.LogInformation("InventoryReserved consumer hosted service stopped successfully.");
    }
}