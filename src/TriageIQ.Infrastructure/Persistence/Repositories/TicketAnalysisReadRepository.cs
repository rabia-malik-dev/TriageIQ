using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Triage;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class TicketAnalysisReadRepository(TriageIQDbContext db) : ITicketAnalysisReadRepository
{
    public async Task<TicketAnalysisDto?> GetLatestByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var row = await (
                from analysis in db.TicketAnalyses.AsNoTracking()
                join call in db.LlmCalls.AsNoTracking() on analysis.LlmCallId equals call.Id
                where analysis.TicketId == ticketId
                orderby analysis.CreatedAt descending
                select new { analysis, call })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return Map(row.analysis, row.call);
    }

    private static TicketAnalysisDto Map(TicketAnalysis analysis, LlmCall call) =>
        new(
            analysis.Id,
            analysis.TicketId,
            analysis.Category,
            analysis.Priority,
            analysis.Summary,
            analysis.Confidence,
            analysis.RoutingDecision,
            call.Provider,
            call.Model,
            call.InputTokens,
            call.OutputTokens,
            call.LatencyMs,
            call.EstimatedCostUsd,
            analysis.ReviewReason,
            analysis.CreatedAt);
}