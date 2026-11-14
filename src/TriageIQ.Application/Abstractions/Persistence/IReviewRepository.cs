using TriageIQ.Application.Reviews;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IReviewRepository
{
    Task<IReadOnlyList<ReviewQueueItemDto>> GetQueueAsync(CancellationToken cancellationToken);

    Task SaveReviewAsync(Ticket ticket, ReviewAction action, CancellationToken cancellationToken);

    Task UpsertFeedbackExampleAsync(Ticket ticket, ReviewAction action, CancellationToken cancellationToken);
}