using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken);

    Task MarkPublishedAsync(Guid outboxId, DateTimeOffset publishedAt, CancellationToken cancellationToken);

    Task MarkFailedAsync(Guid outboxId, string error, CancellationToken cancellationToken);

    Task<int> CountPendingAsync(CancellationToken cancellationToken);
}