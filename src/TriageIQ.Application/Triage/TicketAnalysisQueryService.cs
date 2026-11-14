using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;

namespace TriageIQ.Application.Triage;

public sealed class TicketAnalysisQueryService(ITicketAnalysisReadRepository analyses)
{
    public async Task<TicketAnalysisDto> GetForTicketAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var analysis = await analyses.GetLatestByTicketIdAsync(ticketId, cancellationToken)
            ?? throw new NotFoundException("No analysis exists for this ticket.");

        return analysis;
    }
}