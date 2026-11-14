using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IPromptVersionRepository
{
    Task<PromptVersion?> GetActiveAsync(CancellationToken cancellationToken);

    Task<PromptVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PromptVersion>> ListAllAsync(CancellationToken cancellationToken);

    Task AddAsync(PromptVersion promptVersion, CancellationToken cancellationToken);

    Task SetActiveAsync(Guid id, CancellationToken cancellationToken);

    Task EnsureDefaultSeedAsync(CancellationToken cancellationToken);
}