using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TriageIQ.Infrastructure.Observability;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddTriageOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string serviceName)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return services;
        }

        var endpoint = configuration["OpenTelemetry:OtlpEndpoint"];
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(endpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("TriageIQ");

                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(endpoint));
                }
            });

        return services;
    }
}