using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

public static class RabbitMqDependencyInjection
{
    public static IServiceCollection AddRabbitMqConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.HostName), "RabbitMQ HostName must be configured.")
            .Validate(options => options.Port > 0, "RabbitMQ Port must be greater than zero.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMQ UserName must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMQ Password must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.VirtualHost), "RabbitMQ VirtualHost must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ExchangeName), "RabbitMQ ExchangeName must be configured.")
            .Validate(options => options.PrefetchCount > 0, "RabbitMQ PrefetchCount must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<RabbitMqChannelFactory>();
        services.AddSingleton<RabbitMqTopologyInitializer>();

        return services;
    }
}
