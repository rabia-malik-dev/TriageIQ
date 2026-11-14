using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class PromptVersionRepository(TriageIQDbContext db) : IPromptVersionRepository
{
    public const string DefaultPromptName = "ticket-triage";
    public const string DefaultPromptVersion = "v1";

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