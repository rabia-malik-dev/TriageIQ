using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class DlqRepository(TriageIQDbContext db) : IDlqRepository
{
    public async Task RecordAsync(DlqMessage message, CancellationToken cancellationToken)
    {
        db.DlqMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DlqMessage>> ListActiveAsync(CancellationToken cancellationToken) =>
        await db.DlqMessages.AsNoTracking()
            .Where(m => m.DiscardedAt == null && m.ReplayedAt == null)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<DlqMessage?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await db.DlqMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}