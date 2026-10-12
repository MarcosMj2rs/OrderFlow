using OrderFlow.Application.DependencyInjection;
using OrderFlow.Infrastructure.DependencyInjection;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Worker.Payments.Consumers;
using OrderFlow.Worker.Payments.HostedServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();

builder.Services.AddPersistence(builder.Configuration,
    new InfrastructureOptions
    {
        EnableEfCoreLogging = builder.Environment.IsDevelopment()
    });

builder.Services.AddRabbitMqConfiguration(builder.Configuration);

builder.Services.AddHostedService<RabbitMqTopologyHostedService>();

builder.Services.AddSingleton<OrderCreatedConsumer>();
builder.Services.AddHostedService<OrderCreatedConsumerHostedService>();

builder.Services.AddSingleton<PaymentCompletedConsumer>();
builder.Services.AddHostedService<PaymentCompletedConsumerHostedService>();

builder.Services.AddSingleton<InventoryReservedConsumer>();
builder.Services.AddHostedService<InventoryReservedConsumerHostedService>();

var host = builder.Build();

host.Run();