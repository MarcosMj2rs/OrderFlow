using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence.Context;
using OrderFlow.Infrastructure.Persistence.Repositories;
using OrderFlow.Infrastructure.Persistence.UnitOfWork;

namespace OrderFlow.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("OrderFlowDatabase");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'OrderFlowDatabase' was not configured");

            services.AddDbContext<OrderFlowDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderReadRepository, OrderReadRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
