using OrderFlow.Worker.Payments.Consumers;

namespace OrderFlow.Worker.Payments.HostedServices;

public sealed class PaymentCompletedConsumerHostedService : IHostedService
{
    private readonly PaymentCompletedConsumer _consumer;
    private readonly ILogger<PaymentCompletedConsumerHostedService> _logger;

    public PaymentCompletedConsumerHostedService(PaymentCompletedConsumer consumer,
                                                 ILogger<PaymentCompletedConsumerHostedService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting PaymentCompleted consumer hosted service...");

        await _consumer.StartAsync(cancellationToken);

        _logger.LogInformation("PaymentCompleted consumer hosted service started successfully.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping PaymentCompleted consumer hosted service...");

        await _consumer.DisposeAsync();

        _logger.LogInformation("PaymentCompleted consumer hosted service stopped successfully.");
    }
}