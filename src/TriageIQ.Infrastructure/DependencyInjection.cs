using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TriageIQ.Application.Abstractions;
using TriageIQ.Infrastructure.Health;
using TriageIQ.Infrastructure.Time;

namespace TriageIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeExternalDependencies)
    {
        services.AddSingleton<IClock, SystemClock>();

        var healthChecks = services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live", "ready"]);

        if (includeExternalDependencies)
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                healthChecks.AddSqlServer(
                    connectionString,
                    name: "sqlserver",
                    tags: ["ready"]);
            }

            healthChecks.AddCheck<ServiceBusEmulatorHealthCheck>(
                "servicebus",
                tags: ["ready"]);
        }

        return services;
    }
}