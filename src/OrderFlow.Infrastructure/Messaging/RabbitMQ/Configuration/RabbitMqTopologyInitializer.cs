using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

public sealed class RabbitMqTopologyInitializer
{
    private readonly RabbitMqChannelFactory _channelFactory;
    private readonly RabbitMqOptions _options;

    public RabbitMqTopologyInitializer(RabbitMqChannelFactory channelFactory, IOptions<RabbitMqOptions> options)
    {
        _channelFactory = channelFactory;
        _options = options.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using IChannel channel = await _channelFactory.CreateChanelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(exchange: _options.ExchangeName,
                                           type: ExchangeType.Topic,
                                           durable: true,
                                           autoDelete: false,
                                           arguments: null,
                                           cancellationToken: cancellationToken);
    }
}
