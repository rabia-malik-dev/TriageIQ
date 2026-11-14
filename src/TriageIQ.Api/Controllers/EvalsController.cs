using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Application.Evals;
using TriageIQ.Application.Prompts;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/evals")]
[Authorize(Roles = "Admin")]
public sealed class EvalsController(EvalRunnerService evals, PromptManagementService prompts) : ControllerBase
{
    [HttpPost("run")]
    [ProducesResponseType(typeof(EvalRunDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EvalRunDto>> Run(
        [FromBody] EvalRunRequest request,
        CancellationToken cancellationToken) =>
        Ok(await evals.RunAsync(request, cancellationToken));

    [HttpGet("runs")]
    [ProducesResponseType(typeof(IReadOnlyList<EvalRunDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EvalRunDto>>> List(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await evals.ListAsync(take, cancellationToken));

    [HttpGet("compare")]
    [ProducesResponseType(typeof(PromptCompareResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptCompareResultDto>> Compare(
        [FromQuery] Guid leftRunId,
        [FromQuery] Guid rightRunId,
        CancellationToken cancellationToken)
    {
        var result = await prompts.CompareEvalRunsAsync(leftRunId, rightRunId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}