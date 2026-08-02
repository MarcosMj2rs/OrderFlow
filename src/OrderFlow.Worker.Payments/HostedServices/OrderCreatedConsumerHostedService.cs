using OrderFlow.Worker.Payments.Consumers;

namespace OrderFlow.Worker.Payments.HostedServices;

public sealed class OrderCreatedConsumerHostedService : IHostedService
{
    private readonly OrderCreatedConsumer _consumer;
    private readonly ILogger<OrderCreatedConsumerHostedService> _logger;

    public OrderCreatedConsumerHostedService(OrderCreatedConsumer consumer,
                                             ILogger<OrderCreatedConsumerHostedService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting OrderCreated consumer hosted service...");

        await _consumer.StartAsync(cancellationToken);

        _logger.LogInformation("OrderCreated consumer hosted service started successfully.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping OrderCreated consumer hosted service...");

        await _consumer.DisposeAsync();

        _logger.LogInformation("OrderCreated consumer hosted service stopped successfully.");
    }
}
