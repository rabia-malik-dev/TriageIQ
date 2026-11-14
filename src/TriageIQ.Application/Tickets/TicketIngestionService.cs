using Microsoft.Extensions.Logging;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;
using TriageIQ.Application.Messaging;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Tickets;

public sealed class TicketIngestionService(
    ITicketRepository tickets,
    ILogger<TicketIngestionService> logger)
{
    public async Task ProcessTicketReceivedAsync(
        TicketReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var ticket = await tickets.FindByIdForUpdateAsync(message.TicketId, cancellationToken)
            ?? throw new NotFoundException($"Ticket {message.TicketId} was not found.");

        if (ticket.Status == TicketStatus.QueuedForTriage)
        {
            logger.LogInformation(
                "Ticket {TicketId} already queued for triage; skipping duplicate delivery",
                ticket.Id);
            return;
        }

        ticket.Status = TicketStatus.QueuedForTriage;
        await tickets.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Ticket {TicketId} queued for triage (correlation {CorrelationId})",
            ticket.Id,
            message.CorrelationId);
    }
}