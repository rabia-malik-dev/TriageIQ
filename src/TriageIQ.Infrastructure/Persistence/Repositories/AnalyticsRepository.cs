using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Analytics;
using TriageIQ.Domain.Enums;
using ReviewActionType = TriageIQ.Domain.Enums.ReviewActionType;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class AnalyticsRepository(TriageIQDbContext db) : IAnalyticsRepository
{
    public async Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var tickets = await db.Tickets.AsNoTracking().ToListAsync(cancellationToken);
        var analyses = await db.TicketAnalyses.AsNoTracking().ToListAsync(cancellationToken);
        var calls = await db.LlmCalls.AsNoTracking().ToListAsync(cancellationToken);
        var reviewActions = await db.ReviewActions.AsNoTracking()
            .Where(r => r.Action == ReviewActionType.Edit)
            .CountAsync(cancellationToken);

        var latencies = calls.Select(c => c.LatencyMs).OrderBy(v => v).ToList();
        var p95 = latencies.Count == 0
            ? 0
            : latencies[(int)Math.Ceiling(latencies.Count * 0.95) - 1];

        var avgConfidence = analyses.Count == 0
            ? 0
            : analyses.Average(a => a.Confidence);

        var overrideRate = analyses.Count == 0
            ? 0
            : (decimal)reviewActions / analyses.Count;

        return new AnalyticsSummaryDto(
            tickets.Count,
            tickets.Count(t => t.Status == TicketStatus.AutoApproved),
            tickets.Count(t => t.Status == TicketStatus.NeedsReview),
            tickets.Count(t => t.Status == TicketStatus.Failed),
            tickets.Count(t => t.Status == TicketStatus.ReviewApproved),
            tickets.Count(t => t.Status == TicketStatus.Rejected),
            Math.Round(avgConfidence, 4),
            calls.Sum(c => c.EstimatedCostUsd),
            p95,
            Math.Round(overrideRate, 4));
    }

    public async Task<IReadOnlyList<AnalyticsTimeseriesPointDto>> GetTimeseriesAsync(
        int days,
        CancellationToken cancellationToken)
    {
        days = Math.Clamp(days, 1, 90);
        var from = DateTimeOffset.UtcNow.Date.AddDays(-days + 1);

        var tickets = await db.Tickets.AsNoTracking()
            .Where(t => t.CreatedAt >= from)
            .ToListAsync(cancellationToken);

        var points = new List<AnalyticsTimeseriesPointDto>(days);
        for (var i = 0; i < days; i++)
        {
            var day = DateOnly.FromDateTime(from.Date.AddDays(i));
            var dayTickets = tickets.Where(t => DateOnly.FromDateTime(t.CreatedAt.Date) == day).ToList();
            points.Add(new AnalyticsTimeseriesPointDto(
                day,
                dayTickets.Count,
                dayTickets.Count(t => t.Status == TicketStatus.AutoApproved),
                dayTickets.Count(t => t.Status == TicketStatus.NeedsReview)));
        }

        return points;
    }
}