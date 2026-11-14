using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Common;
using TriageIQ.Application.Exceptions;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Tickets;

public sealed class TicketService(
    ITicketRepository tickets,
    IClock clock)
{
    public async Task<TicketDto> CreateAsync(
        string idempotencyKey,
        CreateTicketRequest request,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Subject = request.Subject.Trim(),
            Body = request.Body,
            Source = request.Source.Trim(),
            Status = TicketStatus.Received,
            IdempotencyKey = idempotencyKey,
            CorrelationId = correlationId,
            CreatedAt = clock.UtcNow,
        };

        var stored = await tickets.GetOrCreateAsync(ticket, cancellationToken);
        return MapToDto(stored);
    }

    public async Task<IReadOnlyList<TicketDto>> CreateBulkAsync(
        BulkCreateTicketsRequest request,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var results = new List<TicketDto>(request.Tickets.Count);

        foreach (var item in request.Tickets)
        {
            var dto = await CreateAsync(
                item.IdempotencyKey,
                new CreateTicketRequest(item.Subject, item.Body, item.Source),
                correlationId,
                cancellationToken);
            results.Add(dto);
        }

        return results;
    }

    public async Task<TicketDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await tickets.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Ticket was not found.");

        return MapToDto(ticket);
    }

    public async Task<PagedResult<TicketDto>> SearchAsync(
        TicketStatus? status,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var criteria = new TicketSearchCriteria(status, createdFrom, createdTo, page, pageSize);
        var (items, total) = await tickets.SearchAsync(criteria, cancellationToken);

        return new PagedResult<TicketDto>(
            items.Select(MapToDto).ToList(),
            page,
            pageSize,
            total);
    }

    private static TicketDto MapToDto(Ticket ticket) =>
        new(
            ticket.Id,
            ticket.Subject,
            ticket.Body,
            ticket.Source,
            ticket.Status,
            ticket.IdempotencyKey,
            ticket.CorrelationId,
            ticket.CreatedAt);
}