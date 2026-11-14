using TriageIQ.Application.Triage;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface ITicketAnalysisReadRepository
{
    Task<TicketAnalysisDto?> GetLatestByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken);
}