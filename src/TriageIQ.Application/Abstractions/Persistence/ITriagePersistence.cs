using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface ITriagePersistence
{
    Task<bool> HasAnalysisAsync(Guid ticketId, CancellationToken cancellationToken);

    Task SaveTriageResultAsync(
        Ticket ticket,
        LlmCall llmCall,
        TicketAnalysis analysis,
        CancellationToken cancellationToken);
}