using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Api.Services;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Exceptions;
using TriageIQ.Application.Reviews;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize(Roles = "Reviewer,Admin")]
public sealed class ReviewsController(ReviewService reviews, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("queue")]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewQueueItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewQueueItemDto>>> Queue(CancellationToken cancellationToken) =>
        Ok(await reviews.GetQueueAsync(cancellationToken));

    [HttpPost("{ticketId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(Guid ticketId, CancellationToken cancellationToken)
    {
        var reviewerId = currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        await reviews.ApproveAsync(ticketId, reviewerId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{ticketId:guid}/edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Edit(
        Guid ticketId,
        [FromBody] ReviewEditRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerId = currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        await reviews.EditAsync(ticketId, reviewerId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{ticketId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(
        Guid ticketId,
        [FromBody] ReviewRejectRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerId = currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        await reviews.RejectAsync(ticketId, reviewerId, request, cancellationToken);
        return NoContent();
    }
}