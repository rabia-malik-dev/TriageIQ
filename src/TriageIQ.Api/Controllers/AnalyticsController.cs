using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Application.Analytics;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Viewer,Reviewer,Admin")]
public sealed class AnalyticsController(AnalyticsQueryService analytics) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalyticsSummaryDto>> Summary(CancellationToken cancellationToken) =>
        Ok(await analytics.GetSummaryAsync(cancellationToken));

    [HttpGet("timeseries")]
    [ProducesResponseType(typeof(IReadOnlyList<AnalyticsTimeseriesPointDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AnalyticsTimeseriesPointDto>>> Timeseries(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default) =>
        Ok(await analytics.GetTimeseriesAsync(days, cancellationToken));
}