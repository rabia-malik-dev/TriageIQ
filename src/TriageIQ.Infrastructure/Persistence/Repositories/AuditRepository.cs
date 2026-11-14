using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class AuditRepository(TriageIQDbContext db, IClock clock) : IAuditRepository
{
    public async Task WriteAsync(
        Guid? userId,
        string action,
        string entity,
        string? entityId,
        string? details,
        CancellationToken cancellationToken)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Details = details,
            CreatedAt = clock.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}