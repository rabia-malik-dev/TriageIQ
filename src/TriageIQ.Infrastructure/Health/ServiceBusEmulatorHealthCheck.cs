using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TriageIQ.Infrastructure.Health;

public sealed class ServiceBusEmulatorHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var healthUrl = configuration["ServiceBus:EmulatorHealthUrl"];
        if (string.IsNullOrWhiteSpace(healthUrl))
        {
            return HealthCheckResult.Healthy("Service Bus health URL not configured; skipping check.");
        }

        try
        {
            var client = httpClientFactory.CreateClient(nameof(ServiceBusEmulatorHealthCheck));
            using var response = await client.GetAsync(healthUrl, cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Service Bus emulator is reachable.")
                : HealthCheckResult.Unhealthy($"Service Bus emulator returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Service Bus emulator is not reachable.", ex);
        }
    }
}