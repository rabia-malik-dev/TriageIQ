using TriageIQ.Domain.Enums;

namespace TriageIQ.Domain.Entities;

public sealed class TicketAnalysis
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid PromptVersionId { get; set; }

    public Guid LlmCallId { get; set; }

    public required string Category { get; set; }

    public required string Priority { get; set; }

    public required string Summary { get; set; }

    public decimal Confidence { get; set; }

    public TriageRoutingDecision RoutingDecision { get; set; }

    public required string RawResponseJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Ticket? Ticket { get; set; }
}