using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Persistence.Context;
using OrderFlow.Infrastructure.Persistence.DomainEvents;
using OrderFlow.Infrastructure.Persistence.Repositories;
using OrderFlow.Infrastructure.Persistence.UnitOfWork;
using System.Diagnostics;

namespace OrderFlow.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                       IConfiguration configuration,
                                                       InfrastructureOptions infrastructureOptions)
    {
        services.AddRabbitMqConfiguration(configuration);

        var connectionString = configuration.GetConnectionString("OrderFlowDatabase");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'OrderFlowDatabase' was not configured");

        services.AddDbContext<OrderFlowDbContext>(options =>
        {
            options.UseSqlServer(connectionString);

            if (infrastructureOptions.EnableEfCoreLogging)
            {
                options
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Information);
            }
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IDomainEventCollector, EfCoreDomainEventCollector>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
