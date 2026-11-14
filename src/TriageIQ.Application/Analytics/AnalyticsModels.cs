namespace TriageIQ.Application.Analytics;

public sealed record AnalyticsSummaryDto(
    int TotalTickets,
    int AutoApproved,
    int NeedsReview,
    int Failed,
    int ReviewApproved,
    int Rejected,
    decimal AverageConfidence,
    decimal TotalCostUsd,
    int P95LatencyMs,
    decimal ReviewOverrideRate);

public sealed record AnalyticsTimeseriesPointDto(
    DateOnly Date,
    int TicketsCreated,
    int AutoApproved,
    int NeedsReview);