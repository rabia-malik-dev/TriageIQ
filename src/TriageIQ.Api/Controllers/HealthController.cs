using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet("live")]
    public Task<IActionResult> Live(CancellationToken cancellationToken) =>
        RunAsync(["live"], cancellationToken);

    [HttpGet("ready")]
    public Task<IActionResult> Ready(CancellationToken cancellationToken) =>
        RunAsync(["ready"], cancellationToken);

    private async Task<IActionResult> RunAsync(
        IEnumerable<string> tags,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Any(tags.Contains),
            cancellationToken);

        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                }),
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(payload)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}