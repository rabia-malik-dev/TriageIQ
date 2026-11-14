using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class PromptVersionRepository(TriageIQDbContext db) : IPromptVersionRepository
{
    public const string DefaultPromptName = "ticket-triage";
    public const string DefaultPromptVersion = "v1";

    public Task<PromptVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.PromptVersions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PromptVersion>> ListAllAsync(CancellationToken cancellationToken) =>
        await db.PromptVersions.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(PromptVersion promptVersion, CancellationToken cancellationToken)
    {
        db.PromptVersions.Add(promptVersion);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetActiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var all = await db.PromptVersions.ToListAsync(cancellationToken);
        foreach (var prompt in all)
        {
            prompt.IsActive = prompt.Id == id;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PromptVersion?> GetActiveAsync(CancellationToken cancellationToken)
    {
        var items = await db.PromptVersions.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        return items
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();
    }

    public async Task EnsureDefaultSeedAsync(CancellationToken cancellationToken)
    {
        if (await db.PromptVersions.AnyAsync(cancellationToken))
        {
            return;
        }

        db.PromptVersions.Add(new PromptVersion
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000101"),
            Name = DefaultPromptName,
            Version = DefaultPromptVersion,
            Template = """
                Classify the support ticket below.

                Subject: {{subject}}
                Source: {{source}}
                Body:
                {{body}}
                """,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}