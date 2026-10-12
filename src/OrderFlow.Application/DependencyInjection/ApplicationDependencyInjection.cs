using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Application.Abstractions.Sagas;
using OrderFlow.Application.Behaviors;
using OrderFlow.Application.Sagas.Order;
using OrderFlow.Application.Services.Messaging;
using System.Reflection;

namespace OrderFlow.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IOrderSagaOrchestrator, OrderSagaOrchestrator>();

        return services;
    }
}