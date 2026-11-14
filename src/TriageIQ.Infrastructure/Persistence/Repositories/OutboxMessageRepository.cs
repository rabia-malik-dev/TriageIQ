using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class OutboxMessageRepository(TriageIQDbContext db) : IOutboxMessageRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var pending = await db.OutboxMessages
            .Where(o => o.Status == OutboxMessageStatus.Pending)
            .ToListAsync(cancellationToken);

        return pending
            .OrderBy(o => o.CreatedAt)
            .Take(batchSize)
            .ToList();
    }

    public async Task MarkPublishedAsync(Guid outboxId, DateTimeOffset publishedAt, CancellationToken cancellationToken)
    {
        var message = await db.OutboxMessages.FirstAsync(o => o.Id == outboxId, cancellationToken);
        message.Status = OutboxMessageStatus.Published;
        message.PublishedAt = publishedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(Guid outboxId, string error, CancellationToken cancellationToken)
    {
        var message = await db.OutboxMessages.FirstAsync(o => o.Id == outboxId, cancellationToken);
        message.AttemptCount++;
        message.LastError = error.Length > 2048 ? error[..2048] : error;

        message.Status = message.AttemptCount >= 5
            ? OutboxMessageStatus.Failed
            : OutboxMessageStatus.Pending;

        await db.SaveChangesAsync(cancellationToken);
    }
}