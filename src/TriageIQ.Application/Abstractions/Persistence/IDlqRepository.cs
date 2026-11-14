using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IDlqRepository
{
    Task RecordAsync(DlqMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyList<DlqMessage>> ListActiveAsync(CancellationToken cancellationToken);

    Task<DlqMessage?> FindByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}