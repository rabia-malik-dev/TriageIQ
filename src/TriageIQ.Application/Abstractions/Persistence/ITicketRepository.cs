using TriageIQ.Application.Tickets;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task<Ticket?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task<Ticket?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Ticket> GetOrCreateAsync(Ticket ticket, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<(IReadOnlyList<Ticket> Items, int Total)> SearchAsync(
        TicketSearchCriteria criteria,
        CancellationToken cancellationToken);
}

public sealed record TicketSearchCriteria(
    TicketStatus? Status,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedTo,
    int Page,
    int PageSize);