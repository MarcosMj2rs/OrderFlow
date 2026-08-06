using Microsoft.Extensions.Options;
using OrderFlow.Worker.Outbox.Configuration;
using OrderFlow.Worker.Outbox.Services;

namespace OrderFlow.Worker.Outbox.HostedServices;

public sealed class OutboxPublisherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxPublisherHostedService> _logger;

    public OutboxPublisherHostedService(IServiceScopeFactory scopeFactory,
                                        IOptions<OutboxOptions> options,
                                        ILogger<OutboxPublisherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(@"Outbox publisher hosted service started. 
                                 BatchSize: {BatchSize}, 
                                 PollingIntervalMilliseconds: {PollingIntervalMilliseconds}",
                                 _options.BatchSize,
                                 _options.PollingIntervalMilliseconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

                OutboxPublisherService publisherService = scope.ServiceProvider.GetRequiredService<OutboxPublisherService>();

                await publisherService.PublishPendingMessagesAsync(_options.BatchSize, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while processing Outbox messages.");
            }

            await Task.Delay(_options.PollingIntervalMilliseconds, stoppingToken);
        }

        _logger.LogInformation("Outbox publisher hosted service stopped.");
    }
}