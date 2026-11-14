using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IPromptVersionRepository
{
    Task<PromptVersion?> GetActiveAsync(CancellationToken cancellationToken);

    Task EnsureDefaultSeedAsync(CancellationToken cancellationToken);
}