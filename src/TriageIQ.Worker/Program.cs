using TriageIQ.Application;
using TriageIQ.Infrastructure;
using TriageIQ.Infrastructure.Observability;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddTriageOpenTelemetry(builder.Configuration, builder.Environment, "TriageIQ.Worker");
builder.Services.AddTicketConsumption();

var host = builder.Build();

if (builder.Configuration.GetConnectionString("Default") is { Length: > 0 })
{
    await host.Services.ApplyMigrationsAsync(CancellationToken.None);

    await using var scope = host.Services.CreateAsyncScope();
    var prompts = scope.ServiceProvider.GetRequiredService<IPromptVersionRepository>();
    await prompts.EnsureDefaultSeedAsync(CancellationToken.None);
}

host.Run();