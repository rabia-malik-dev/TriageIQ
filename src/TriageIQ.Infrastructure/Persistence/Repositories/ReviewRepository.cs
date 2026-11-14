using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Reviews;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository(TriageIQDbContext db) : IReviewRepository
{
    public async Task<IReadOnlyList<ReviewQueueItemDto>> GetQueueAsync(CancellationToken cancellationToken)
    {
        var tickets = (await db.Tickets.AsNoTracking()
            .Where(t => t.Status == TicketStatus.NeedsReview)
            .ToListAsync(cancellationToken))
            .OrderBy(t => t.CreatedAt)
            .ToList();

        if (tickets.Count == 0)
        {
            return [];
        }

        var ticketIds = tickets.Select(t => t.Id).ToList();
        var analyses = (await db.TicketAnalyses.AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .ToListAsync(cancellationToken))
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        var byTicket = new Dictionary<Guid, TicketAnalysis>();
        foreach (var analysis in analyses)
        {
            byTicket.TryAdd(analysis.TicketId, analysis);
        }

        return tickets.Select(ticket =>
        {
            byTicket.TryGetValue(ticket.Id, out var analysis);
            return new ReviewQueueItemDto(
                ticket.Id,
                ticket.Subject,
                ticket.Status,
                analysis?.Category,
                analysis?.Priority,
                analysis?.Summary,
                analysis?.Confidence,
                analysis?.ReviewReason,
                ticket.CreatedAt);
        }).ToList();
    }

    public async Task SaveReviewAsync(Ticket ticket, ReviewAction action, CancellationToken cancellationToken)
    {
        db.ReviewActions.Add(action);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertFeedbackExampleAsync(Ticket ticket, ReviewAction action, CancellationToken cancellationToken)
    {
        var feedbackDir = ResolveFeedbackDirectory();
        Directory.CreateDirectory(feedbackDir);
        var path = Path.Combine(feedbackDir, "feedback-export.jsonl");

        var line = JsonSerializer.Serialize(new
        {
            ticketId = ticket.Id,
            subject = ticket.Subject,
            category = action.EditedCategory,
            priority = action.EditedPriority,
            reply = action.EditedReply,
            exportedAt = DateTimeOffset.UtcNow,
        });

        await File.AppendAllTextAsync(path, line + Environment.NewLine, cancellationToken);
    }

    private static string ResolveFeedbackDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "eval", "datasets");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "eval", "datasets");
    }
}