using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Worker.Payments.Consumers;
using OrderFlow.Worker.Payments.HostedServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqConfiguration(builder.Configuration);

builder.Services.AddSingleton<OrderCreatedConsumer>();
builder.Services.AddHostedService<OrderCreatedConsumerHostedService>();

var host = builder.Build();

host.Run();
