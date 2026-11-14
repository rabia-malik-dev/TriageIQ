using Microsoft.Extensions.Logging;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Messaging;
using TriageIQ.Application.Abstractions.Persistence;

namespace TriageIQ.Application.Messaging;

public sealed class OutboxDispatcher(
    IOutboxMessageRepository outbox,
    IMessageBus bus,
    IClock clock,
    ILogger<OutboxDispatcher> logger) : IOutboxDispatcher
{
    private const int BatchSize = 20;

    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var pending = await outbox.GetPendingBatchAsync(BatchSize, cancellationToken);
        var published = 0;

        foreach (var message in pending)
        {
            try
            {
                await bus.PublishAsync(
                    TicketMessaging.IncomingQueueName,
                    message.Id.ToString(),
                    message.PayloadJson,
                    message.CorrelationId,
                    cancellationToken);

                await outbox.MarkPublishedAsync(message.Id, clock.UtcNow, cancellationToken);
                published++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to publish outbox message {OutboxId} for ticket {TicketId}",
                    message.Id,
                    message.TicketId);

                await outbox.MarkFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }

        return published;
    }
}