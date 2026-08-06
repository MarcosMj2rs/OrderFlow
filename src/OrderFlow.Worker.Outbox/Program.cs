using OrderFlow.Infrastructure.DependencyInjection;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Worker.Outbox.Configuration;
using OrderFlow.Worker.Outbox.HostedServices;
using OrderFlow.Worker.Outbox.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPersistence(builder.Configuration, new InfrastructureOptions
{
    EnableEfCoreLogging = builder.Environment.IsDevelopment()
});

builder.Services.AddRabbitMqConfiguration(builder.Configuration);

builder.Services
    .AddOptions<OutboxOptions>()
    .Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
    .Validate(options => options.BatchSize > 0, "Outbox BatchSize must be greater than zero.")
    .Validate(options => options.PollingIntervalMilliseconds > 0, "Outbox PollingIntervalMilliseconds must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddScoped<OutboxPublisherService>();

builder.Services.AddHostedService<OutboxPublisherHostedService>();

var host = builder.Build();

host.Run();