using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderFlow.Infrastructure.Messaging.Exceptions;
using System.Text;

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
    private IChannel? _publishChannel;
    private string? _consumerTag;
    private bool _disposed;
    private const string RetryCountHeader = "x-orderflow-retry-count";

    protected RabbitMqConsumerBase(RabbitMqChannelFactory channelFactory,
                                   IOptions<RabbitMqOptions> options,
                                   ILogger logger)
    {
        _channelFactory = channelFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected abstract string QueueName { get; }
    protected abstract string RetryRoutingKey { get; }
    protected abstract string DeadLetterRoutingKey { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_channel is { IsOpen: true })
            return;

        _channel = await _channelFactory.CreateChannelAsync(cancellationToken: cancellationToken);

        _publishChannel = await _channelFactory.CreateChannelAsync(publisherConfirmationsEnabled: true,
                                                                   cancellationToken: cancellationToken);

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

            await HandlePermanentFailureAsync(eventArgs, exception);
        }
        catch (PermanentMessagingException exception)
        {
            _logger.LogError(exception,
                             "Permanent messaging failure. Queue: {QueueName}. DeliveryTag: {DeliveryTag}",
                             QueueName,
                             eventArgs.DeliveryTag);

            await HandlePermanentFailureAsync(eventArgs, exception);
        }
        catch (TransientMessagingException exception)
        {
            _logger.LogWarning(exception,
                               "Transient messaging failure. Queue: {QueueName}. DeliveryTag: {DeliveryTag}",
                               QueueName,
                               eventArgs.DeliveryTag);

            await HandleTransientFailureAsync(eventArgs, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                             "Unexpected error processing message. Queue: {QueueName}. DeliveryTag: {DeliveryTag}",
                             QueueName,
                             eventArgs.DeliveryTag);

            await HandleTransientFailureAsync(eventArgs, exception);
        }
    }

    protected abstract Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is null)
            return;

        if (_publishChannel is not null)
        {
            if (_publishChannel.IsOpen)
                await _publishChannel.CloseAsync(cancellationToken: cancellationToken);

            await _publishChannel.DisposeAsync();

            _publishChannel = null;
        }

        if (!string.IsNullOrWhiteSpace(_consumerTag))
        {
            await _channel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);
            _consumerTag = null;
        }

        if (_channel.IsOpen)
            await _channel.CloseAsync(cancellationToken: cancellationToken);

        await _channel.DisposeAsync();

        _channel = null;

        if (_publishChannel is not null)
        {
            if (_publishChannel.IsOpen)
                await _publishChannel.CloseAsync(cancellationToken: cancellationToken);

            await _publishChannel.DisposeAsync();

            _publishChannel = null;
        }

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

    private async Task HandlePermanentFailureAsync(BasicDeliverEventArgs eventArgs, Exception exception)
    {
        int retryCount = GetRetryCount(eventArgs.BasicProperties);

        _logger.LogError(exception,
                         "Publishing message directly to dead-letter queue. Queue: {QueueName}. RetryCount: {RetryCount}. MessageId: {MessageId}",
                         QueueName,
                         retryCount,
                         eventArgs.BasicProperties.MessageId);

        await PublishAndAcknowledgeAsync(eventArgs, DeadLetterRoutingKey, retryCount);
    }

    private async Task HandleTransientFailureAsync(BasicDeliverEventArgs eventArgs, Exception exception)
    {
        int currentRetryCount = GetRetryCount(eventArgs.BasicProperties);

        int nextRetryCount = currentRetryCount + 1;

        if (nextRetryCount > _options.MaxRetryAttempts)
        {
            _logger.LogError(exception,
                             "Maximum retry attempts exceeded. Queue: {QueueName}. RetryCount: {RetryCount}. MessageId: {MessageId}",
                             QueueName,
                             currentRetryCount,
                             eventArgs.BasicProperties.MessageId);

            await PublishAndAcknowledgeAsync(eventArgs, DeadLetterRoutingKey, currentRetryCount);

            return;
        }

        _logger.LogWarning(exception,
                           "Publishing message to retry queue. Queue: {QueueName}. RetryCount: {RetryCount}. MessageId: {MessageId}",
                           QueueName,
                           nextRetryCount,
                           eventArgs.BasicProperties.MessageId);

        await PublishAndAcknowledgeAsync(eventArgs, RetryRoutingKey, nextRetryCount);
    }

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue(RetryCountHeader, out object? value) || value is null)
            return 0;

        return value switch
        {
            byte retryCount => retryCount,
            short retryCount => retryCount,
            int retryCount => retryCount,
            long retryCount => checked((int)retryCount),
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out int parsed) => parsed,
            _ => 0
        };
    }

    private static BasicProperties CreateBasicProperties(IReadOnlyBasicProperties originalProperties, int retryCount)
    {
        Dictionary<string, object?> headers = originalProperties.Headers is null
                ? []
                : new Dictionary<string, object?>(originalProperties.Headers);

        headers[RetryCountHeader] = retryCount;

        return new BasicProperties
        {
            ContentType = originalProperties.ContentType,
            ContentEncoding = originalProperties.ContentEncoding,
            DeliveryMode = originalProperties.DeliveryMode,
            MessageId = originalProperties.MessageId,
            CorrelationId = originalProperties.CorrelationId,
            Type = originalProperties.Type,
            Timestamp = originalProperties.Timestamp,
            AppId = originalProperties.AppId,
            Headers = headers
        };
    }

    private async Task PublishAndAcknowledgeAsync(BasicDeliverEventArgs eventArgs, string routingKey, int retryCount)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitMQ consumer channel was not initialized.");

        if (_publishChannel is null)
            throw new InvalidOperationException("RabbitMQ publisher channel was not initialized.");

        BasicProperties properties = CreateBasicProperties(eventArgs.BasicProperties, retryCount);

        await _publishChannel.BasicPublishAsync(exchange: _options.ExchangeName,
                                                routingKey: routingKey,
                                                mandatory: true,
                                                basicProperties: properties,
                                                body: eventArgs.Body,
                                                cancellationToken: eventArgs.CancellationToken);

        await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag,
                                    multiple: false,
                                    cancellationToken: eventArgs.CancellationToken);
    }
}