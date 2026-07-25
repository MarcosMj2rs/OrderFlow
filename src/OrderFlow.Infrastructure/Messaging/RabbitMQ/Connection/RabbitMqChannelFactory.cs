using RabbitMQ.Client;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;

public sealed class RabbitMqChannelFactory
{
    private readonly RabbitMqConnection _rabbitMqConnection;

    public RabbitMqChannelFactory(RabbitMqConnection rabbitMqConnection)
    {
        _rabbitMqConnection = rabbitMqConnection;
    }

    public async Task<IChannel> CreateChanelAsync(bool publisherConfirmationsEnabled = false,
        CancellationToken cancellationToken = default)
    {
        IConnection connection = await _rabbitMqConnection.GetConnectionAsync(cancellationToken);

        var options = new CreateChannelOptions
            (
                publisherConfirmationsEnabled: publisherConfirmationsEnabled,
                publisherConfirmationTrackingEnabled: publisherConfirmationsEnabled
            );

        return await connection.CreateChannelAsync(options, cancellationToken);
    }
}
