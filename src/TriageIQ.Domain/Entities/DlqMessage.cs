namespace TriageIQ.Domain.Entities;

public sealed class DlqMessage
{
    public Guid Id { get; set; }

    public required string ServiceBusMessageId { get; set; }

    public required string PayloadJson { get; set; }

    public string? ErrorReason { get; set; }

    public int DeliveryCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ReplayedAt { get; set; }

    public DateTimeOffset? DiscardedAt { get; set; }
}