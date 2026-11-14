using TriageIQ.Domain.Enums;

namespace TriageIQ.Domain.Entities;

public sealed class Ticket
{
    public Guid Id { get; set; }

    public required string Subject { get; set; }

    public required string Body { get; set; }

    public string? BodyRedacted { get; set; }

    public required string Source { get; set; }

    public TicketStatus Status { get; set; }

    public required string IdempotencyKey { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}