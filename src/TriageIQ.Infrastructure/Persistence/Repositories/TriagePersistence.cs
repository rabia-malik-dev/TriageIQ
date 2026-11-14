using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class TriagePersistence(TriageIQDbContext db) : ITriagePersistence
{
    public Task<bool> HasAnalysisAsync(Guid ticketId, CancellationToken cancellationToken) =>
        db.TicketAnalyses.AnyAsync(a => a.TicketId == ticketId, cancellationToken);

    public async Task SaveTriageResultAsync(
        Ticket ticket,
        LlmCall llmCall,
        TicketAnalysis analysis,
        CancellationToken cancellationToken)
    {
        await db.LlmCalls.AddAsync(llmCall, cancellationToken);
        await db.TicketAnalyses.AddAsync(analysis, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}