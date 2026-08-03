namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string ExchangeName { get; init; } = "orderflow.events";

    public ushort PrefetchCount { get; init; } = 1;

    public int RetryDelayMilliseconds { get; init; } = 10_000;

    public int MaxRetryAttempts { get; init; } = 3;
}
