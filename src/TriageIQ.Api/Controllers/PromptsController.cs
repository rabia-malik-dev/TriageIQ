using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Application.Prompts;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/prompts")]
[Authorize(Roles = "Admin")]
public sealed class PromptsController(PromptManagementService prompts) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PromptVersionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PromptVersionDto>>> List(CancellationToken cancellationToken) =>
        Ok(await prompts.ListAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(PromptVersionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptVersionDto>> Create(
        [FromBody] CreatePromptVersionRequest request,
        CancellationToken cancellationToken) =>
        Ok(await prompts.CreateAsync(request, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await prompts.ActivateAsync(id, cancellationToken);
        return NoContent();
    }
}