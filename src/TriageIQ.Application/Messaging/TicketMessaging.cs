using System.Text.Json;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Messaging;

public static class TicketMessaging
{
    public const string TicketReceivedType = "TicketReceived";
    public const string IncomingQueueName = "tickets.incoming";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string BuildTicketReceivedPayload(Ticket ticket) =>
        JsonSerializer.Serialize(
            new TicketReceivedMessage(
                ticket.Id,
                ticket.CorrelationId,
                ticket.Subject,
                ticket.Source,
                ticket.CreatedAt),
            JsonOptions);
}

public sealed record TicketReceivedMessage(
    Guid TicketId,
    Guid CorrelationId,
    string Subject,
    string Source,
    DateTimeOffset OccurredAt);