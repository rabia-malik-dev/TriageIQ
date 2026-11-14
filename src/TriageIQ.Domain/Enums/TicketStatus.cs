namespace TriageIQ.Domain.Enums;

public enum TicketStatus
{
    Received = 0,
    QueuedForTriage = 1,
    AutoApproved = 2,
    NeedsReview = 3,
    Failed = 4,
}