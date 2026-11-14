using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Reviews;

public sealed record ReviewQueueItemDto(
    Guid TicketId,
    string Subject,
    TicketStatus Status,
    string? Category,
    string? Priority,
    string? Summary,
    decimal? Confidence,
    string? ReviewReason,
    DateTimeOffset CreatedAt);

public sealed record ReviewEditRequest(
    string Category,
    string Priority,
    string? Reply,
    string? Comment);

public sealed record ReviewRejectRequest(string Comment);