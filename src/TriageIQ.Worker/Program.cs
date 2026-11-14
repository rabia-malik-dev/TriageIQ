using TriageIQ.Application;
using TriageIQ.Infrastructure;
using TriageIQ.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddTicketConsumption();

var host = builder.Build();

if (builder.Configuration.GetConnectionString("Default") is { Length: > 0 })
{
    await host.Services.ApplyMigrationsAsync(CancellationToken.None);
}

host.Run();