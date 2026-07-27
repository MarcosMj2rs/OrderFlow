using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Behaviors;
using System.Reflection;
using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Application.Services.Messaging;

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

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
