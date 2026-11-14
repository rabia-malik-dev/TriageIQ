using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Triage;

public sealed record TriageLlmResponse(
    string Category,
    string Priority,
    string Summary,
    decimal Confidence);

public sealed record TicketAnalysisDto(
    Guid Id,
    Guid TicketId,
    string Category,
    string Priority,
    string Summary,
    decimal Confidence,
    TriageRoutingDecision RoutingDecision,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    int LatencyMs,
    decimal EstimatedCostUsd,
    string? ReviewReason,
    DateTimeOffset CreatedAt);