using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.Infrastructure.Messaging.Consumers;

public abstract class RabbitMqConsumerBase<TMessage> : IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RabbitMqChannelFactory _channelFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger _logger;

    private IChannel? _channel;
    private string? _consumerTag;
    private bool _disposed;

    protected RabbitMqConsumerBase(RabbitMqChannelFactory channelFactory,
                                   IOptions<RabbitMqOptions> options,
                                   ILogger logger)
    {
        _channelFactory = channelFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected abstract string QueueName { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_channel is { IsOpen: true })
            return;

        _channel = await _channelFactory.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(prefetchSize: 0,
                                     prefetchCount: _options.PrefetchCount,
                                     global: false,
                                     cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += HandleMessageAsync;

        _consumerTag = await _channel.BasicConsumeAsync(queue: QueueName,
                                                        autoAck: false,
                                                        consumer: consumer,
                                                        cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ consumer started. Queue: {QueueName}. PrefetchCount: {PrefetchCount}",
                                QueueName,
                                _options.PrefetchCount);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitMQ consumer channel was not initialized.");

        byte[] messageBody = eventArgs.Body.ToArray();

        try
        {
            TMessage? message = JsonSerializer.Deserialize<TMessage>(messageBody, SerializerOptions);

            if (message is null)
                throw new JsonException($"The payload could not be deserialized as {typeof(TMessage).Name}.");

            await ProcessMessageAsync(message, eventArgs.CancellationToken);

            await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag,
                                         multiple: false,
                                         cancellationToken: eventArgs.CancellationToken);

            _logger.LogInformation("RabbitMQ message acknowledged. Queue: {QueueName}. DeliveryTag: {DeliveryTag}",
                                    QueueName,
                                    eventArgs.DeliveryTag);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception,
                             "Invalid payload received. Queue: {QueueName}. DeliveryTag: {DeliveryTag}",
                             QueueName,
                             eventArgs.DeliveryTag);

            await _channel.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag,
                                          multiple: false,
                                          requeue: false,
                                          cancellationToken: eventArgs.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                             "Unexpected error processing message. Queue: {QueueName}. DeliveryTag: {DeliveryTag}",
                             QueueName,
                             eventArgs.DeliveryTag);

            await _channel.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag,
                                          multiple: false,
                                          requeue: true,
                                          cancellationToken: eventArgs.CancellationToken);
        }
    }

    protected abstract Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is null)
            return;

        if (!string.IsNullOrWhiteSpace(_consumerTag))
        {
            await _channel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);
            _consumerTag = null;
        }

        if (_channel.IsOpen)
            await _channel.CloseAsync(cancellationToken: cancellationToken);

        await _channel.DisposeAsync();

        _channel = null;

        _logger.LogInformation("RabbitMQ consumer stopped. Queue: {QueueName}", QueueName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await StopAsync();

        GC.SuppressFinalize(this);
    }
}