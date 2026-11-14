using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Application.Ops;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/ops")]
[Authorize(Roles = "Admin")]
public sealed class OpsController(OpsService ops) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(OpsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OpsSummaryDto>> Summary(CancellationToken cancellationToken) =>
        Ok(await ops.GetSummaryAsync(cancellationToken));

    [HttpGet("dlq")]
    [ProducesResponseType(typeof(IReadOnlyList<DlqMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DlqMessageDto>>> ListDlq(CancellationToken cancellationToken) =>
        Ok(await ops.ListDlqAsync(cancellationToken));

    [HttpPost("dlq/{id:guid}/replay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Replay(Guid id, CancellationToken cancellationToken)
    {
        await ops.ReplayDlqAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("dlq/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Discard(Guid id, CancellationToken cancellationToken)
    {
        await ops.DiscardDlqAsync(id, cancellationToken);
        return NoContent();
    }
}