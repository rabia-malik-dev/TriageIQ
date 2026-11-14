namespace TriageIQ.Domain.Entities;

public sealed class LlmCall
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid CorrelationId { get; set; }

    public Guid PromptVersionId { get; set; }

    public required string Provider { get; set; }

    public required string Model { get; set; }

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int LatencyMs { get; set; }

    public decimal EstimatedCostUsd { get; set; }

    public bool Succeeded { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}