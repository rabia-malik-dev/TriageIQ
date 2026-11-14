using TriageIQ.Domain.Enums;

namespace TriageIQ.Domain.Entities;

public sealed class ReviewAction
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid ReviewerId { get; set; }

    public ReviewActionType Action { get; set; }

    public string? EditedCategory { get; set; }

    public string? EditedPriority { get; set; }

    public string? EditedReply { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}