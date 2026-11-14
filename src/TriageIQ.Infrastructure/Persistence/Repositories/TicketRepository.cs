using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository(TriageIQDbContext db) : ITicketRepository
{
    public Task<Ticket?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);

    public Task<Ticket?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Ticket?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        db.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TicketCreateResult> GetOrCreateAsync(
        Ticket ticket,
        OutboxMessage outboxForNewTicket,
        CancellationToken cancellationToken)
    {
        var existing = await FindByIdempotencyKeyAsync(ticket.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return new TicketCreateResult(existing, IsNew: false);
        }

        await db.Tickets.AddAsync(ticket, cancellationToken);
        await db.OutboxMessages.AddAsync(outboxForNewTicket, cancellationToken);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return new TicketCreateResult(ticket, IsNew: true);
        }
        catch (DbUpdateException ex) when (IsIdempotencyViolation(ex))
        {
            db.ChangeTracker.Clear();
            var duplicate = (await FindByIdempotencyKeyAsync(ticket.IdempotencyKey, cancellationToken))!;
            return new TicketCreateResult(duplicate, IsNew: false);
        }
    }

    private static bool IsIdempotencyViolation(DbUpdateException ex) =>
        ex.InnerException switch
        {
            SqlException sql => sql.Number is 2601 or 2627,
            SqliteException sqlite => sqlite.SqliteErrorCode == 19,
            _ => false,
        };

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<(IReadOnlyList<Ticket> Items, int Total)> SearchAsync(
        TicketSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = db.Tickets.AsNoTracking();

        if (criteria.Status is not null)
        {
            query = query.Where(t => t.Status == criteria.Status);
        }

        if (criteria.CreatedFrom is not null)
        {
            query = query.Where(t => t.CreatedAt >= criteria.CreatedFrom);
        }

        if (criteria.CreatedTo is not null)
        {
            query = query.Where(t => t.CreatedAt <= criteria.CreatedTo);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}