using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class EvalRunRepository(TriageIQDbContext db) : IEvalRunRepository
{
    public async Task<EvalRun> AddAsync(EvalRun run, CancellationToken cancellationToken)
    {
        db.EvalRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    public async Task<IReadOnlyList<EvalRun>> ListRecentAsync(int take, CancellationToken cancellationToken) =>
        await db.EvalRuns.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<EvalRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await db.EvalRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}