using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Application.Triage;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Authorize(Roles = "Viewer,Reviewer,Admin")]
[Route("api/tickets/{ticketId:guid}/analysis")]
public sealed class TicketAnalysisController(TicketAnalysisQueryService analyses) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(TicketAnalysisDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketAnalysisDto>> Get(Guid ticketId, CancellationToken cancellationToken)
    {
        var analysis = await analyses.GetForTicketAsync(ticketId, cancellationToken);
        return Ok(analysis);
    }
}