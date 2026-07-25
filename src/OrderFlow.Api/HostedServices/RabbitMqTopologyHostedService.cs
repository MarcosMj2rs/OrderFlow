using OrderFlow.Api;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

namespace OrderFlow.WebApi.HostedServices
{
    public sealed class RabbitMqTopologyHostedService : IHostedService
    {
        private readonly RabbitMqTopologyInitializer _topologyInitializer;
        private readonly ILogger<RabbitMqTopologyHostedService> _logger;

        public RabbitMqTopologyHostedService(RabbitMqTopologyInitializer topologyInitializer,
                                             ILogger<RabbitMqTopologyHostedService> logger)
        {
            _topologyInitializer = topologyInitializer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ConsoleBanner.RabbitMqProccess("Starting RabbitMQ topology initialization.");

            try
            {
                await _topologyInitializer.InitializeAsync(cancellationToken);

                ConsoleBanner.RabbitMqProccess("RabbitMQ topology initialized successfully.");
            }
            catch (Exception exception)
            {
                ConsoleBanner.RabbitMqError($"RabbitMQ topology initialization failed: {exception.Message}");

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ConsoleBanner.RabbitMqInformation("RabbitMQ topology hosted service stopped.");

            return Task.CompletedTask;
        }
    }
}