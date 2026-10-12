using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

namespace OrderFlow.Worker.Payments.HostedServices;

/// <summary>
/// Initializes the RabbitMQ topology before the consumers start.
/// </summary>
public sealed class RabbitMqTopologyHostedService : IHostedService
{
    private readonly RabbitMqTopologyInitializer _topologyInitializer;
    private readonly ILogger<RabbitMqTopologyHostedService> _logger;

    public RabbitMqTopologyHostedService(RabbitMqTopologyInitializer topologyInitializer, ILogger<RabbitMqTopologyHostedService> logger)
    {
        _topologyInitializer = topologyInitializer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RabbitMQ topology initialization...");

        await _topologyInitializer.InitializeAsync(cancellationToken);

        _logger.LogInformation("RabbitMQ topology initialized successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}