using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IEvalRunRepository
{
    Task<EvalRun> AddAsync(EvalRun run, CancellationToken cancellationToken);

    Task<IReadOnlyList<EvalRun>> ListRecentAsync(int take, CancellationToken cancellationToken);

    Task<EvalRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}