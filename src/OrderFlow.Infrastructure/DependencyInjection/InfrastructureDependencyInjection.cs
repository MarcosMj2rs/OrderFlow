using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

namespace OrderFlow.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                       IConfiguration configuration,
                                                       InfrastructureOptions infrastructureOptions)
    {
        services.AddRabbitMqConfiguration(configuration);

        services.AddPersistence(configuration, infrastructureOptions);

        return services;
    }
}
