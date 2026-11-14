using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Tickets;

public static class TicketLimits
{
    public const int MaxBodyLength = 16_384;
    public const int MaxBulkCount = 100;
    public const int MaxSubjectLength = 500;
    public const int MaxSourceLength = 100;
}

public sealed record CreateTicketRequest(string Subject, string Body, string Source);

public sealed record BulkCreateTicketItemRequest(
    string Subject,
    string Body,
    string Source,
    string IdempotencyKey);

public sealed record BulkCreateTicketsRequest(IReadOnlyList<BulkCreateTicketItemRequest> Tickets);

public sealed record TicketDto(
    Guid Id,
    string Subject,
    string Body,
    string Source,
    TicketStatus Status,
    string IdempotencyKey,
    Guid CorrelationId,
    DateTimeOffset CreatedAt);