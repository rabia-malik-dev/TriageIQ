using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Api.Middleware;
using TriageIQ.Application.Common;
using TriageIQ.Application.Tickets;
using TriageIQ.Application.Validation;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(TicketService ticketService, ValidationService validation) : ControllerBase
{
    [Authorize(Roles = "Reviewer,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketDto>> Create(
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Problem(
                detail: "The Idempotency-Key header is required.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation error");
        }

        await validation.ValidateAndThrowAsync(request, cancellationToken);

        var correlationId = GetCorrelationId();
        var ticket = await ticketService.CreateAsync(
            idempotencyKey.Trim(),
            request,
            correlationId,
            cancellationToken);

        return Ok(ticket);
    }

    [Authorize(Roles = "Reviewer,Admin")]
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> CreateBulk(
        [FromBody] BulkCreateTicketsRequest request,
        CancellationToken cancellationToken)
    {
        await validation.ValidateAndThrowAsync(request, cancellationToken);

        var correlationId = GetCorrelationId();
        var tickets = await ticketService.CreateBulkAsync(request, correlationId, cancellationToken);
        return Ok(tickets);
    }

    [Authorize(Roles = "Viewer,Reviewer,Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketDto>>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] DateTimeOffset? createdFrom,
        [FromQuery] DateTimeOffset? createdTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return Problem(
                detail: "page must be >= 1 and pageSize must be between 1 and 100.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation error");
        }

        var result = await ticketService.SearchAsync(
            status,
            createdFrom,
            createdTo,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Viewer,Reviewer,Admin")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetByIdAsync(id, cancellationToken);
        return Ok(ticket);
    }

    private Guid GetCorrelationId() =>
        HttpContext.Items[CorrelationIdMiddleware.ItemKey] is Guid id ? id : Guid.NewGuid();
}