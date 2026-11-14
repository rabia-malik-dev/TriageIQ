namespace TriageIQ.Application.Abstractions.Persistence;

public interface IAuditRepository
{
    Task WriteAsync(
        Guid? userId,
        string action,
        string entity,
        string? entityId,
        string? details,
        CancellationToken cancellationToken);
}