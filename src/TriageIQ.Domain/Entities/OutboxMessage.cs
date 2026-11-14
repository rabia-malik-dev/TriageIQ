using TriageIQ.Domain.Enums;

namespace TriageIQ.Domain.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid CorrelationId { get; set; }

    public required string MessageType { get; set; }

    public required string PayloadJson { get; set; }

    public OutboxMessageStatus Status { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
}