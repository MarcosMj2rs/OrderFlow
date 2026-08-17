using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Worker.Payments.Consumers;
using OrderFlow.Worker.Payments.HostedServices;
using OrderFlow.Infrastructure.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPersistence(
    builder.Configuration,
    new InfrastructureOptions
    {
        EnableEfCoreLogging = builder.Environment.IsDevelopment()
    });

builder.Services.AddRabbitMqConfiguration(builder.Configuration);

builder.Services.AddSingleton<OrderCreatedConsumer>();
builder.Services.AddHostedService<OrderCreatedConsumerHostedService>();

var host = builder.Build();

host.Run();
