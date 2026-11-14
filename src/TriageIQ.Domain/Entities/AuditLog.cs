namespace TriageIQ.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public required string Action { get; set; }

    public required string Entity { get; set; }

    public string? EntityId { get; set; }

    public string? Details { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}