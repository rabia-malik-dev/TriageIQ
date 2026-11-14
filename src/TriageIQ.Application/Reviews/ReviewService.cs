using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Reviews;

public sealed class ReviewService(
    IReviewRepository reviews,
    ITicketRepository tickets,
    IAuditRepository audit,
    IClock clock)
{
    public async Task<IReadOnlyList<ReviewQueueItemDto>> GetQueueAsync(
        CancellationToken cancellationToken) =>
        await reviews.GetQueueAsync(cancellationToken);

    public async Task ApproveAsync(Guid ticketId, Guid reviewerId, CancellationToken cancellationToken)
    {
        var ticket = await tickets.FindByIdForUpdateAsync(ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket was not found.");

        if (ticket.Status is not (TicketStatus.NeedsReview or TicketStatus.AutoApproved))
        {
            throw new ConflictException("Ticket is not in review.");
        }

        ticket.Status = TicketStatus.ReviewApproved;

        var action = new ReviewAction
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            ReviewerId = reviewerId,
            Action = ReviewActionType.Approve,
            CreatedAt = clock.UtcNow,
        };

        await reviews.SaveReviewAsync(ticket, action, cancellationToken);
        await audit.WriteAsync(reviewerId, "review.approve", "Ticket", ticketId.ToString(), null, cancellationToken);
    }

    public async Task EditAsync(
        Guid ticketId,
        Guid reviewerId,
        ReviewEditRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await tickets.FindByIdForUpdateAsync(ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket was not found.");

        if (ticket.Status is not TicketStatus.NeedsReview)
        {
            throw new ConflictException("Ticket is not awaiting review.");
        }

        ticket.Status = TicketStatus.ReviewApproved;

        var action = new ReviewAction
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            ReviewerId = reviewerId,
            Action = ReviewActionType.Edit,
            EditedCategory = request.Category.Trim(),
            EditedPriority = request.Priority.Trim(),
            EditedReply = request.Reply,
            Comment = request.Comment,
            CreatedAt = clock.UtcNow,
        };

        await reviews.SaveReviewAsync(ticket, action, cancellationToken);
        await reviews.UpsertFeedbackExampleAsync(ticket, action, cancellationToken);
        await audit.WriteAsync(reviewerId, "review.edit", "Ticket", ticketId.ToString(), request.Category, cancellationToken);
    }

    public async Task RejectAsync(
        Guid ticketId,
        Guid reviewerId,
        ReviewRejectRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await tickets.FindByIdForUpdateAsync(ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket was not found.");

        if (ticket.Status is not TicketStatus.NeedsReview)
        {
            throw new ConflictException("Ticket is not awaiting review.");
        }

        ticket.Status = TicketStatus.Rejected;

        var action = new ReviewAction
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            ReviewerId = reviewerId,
            Action = ReviewActionType.Reject,
            Comment = request.Comment,
            CreatedAt = clock.UtcNow,
        };

        await reviews.SaveReviewAsync(ticket, action, cancellationToken);
        await audit.WriteAsync(reviewerId, "review.reject", "Ticket", ticketId.ToString(), request.Comment, cancellationToken);
    }
}