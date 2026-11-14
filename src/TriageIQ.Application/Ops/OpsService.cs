using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Messaging;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;
using TriageIQ.Application.Messaging;

namespace TriageIQ.Application.Ops;

public sealed class OpsService(
    IDlqRepository dlq,
    IOutboxMessageRepository outbox,
    IReviewRepository reviews,
    IMessageBus messageBus,
    IClock clock)
{
    public async Task<IReadOnlyList<DlqMessageDto>> ListDlqAsync(CancellationToken cancellationToken)
    {
        var items = await dlq.ListActiveAsync(cancellationToken);
        return items
            .Select(m => new DlqMessageDto(m.Id, m.ServiceBusMessageId, m.ErrorReason, m.DeliveryCount, m.CreatedAt))
            .ToList();
    }

    public async Task ReplayDlqAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await dlq.FindByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("DLQ message was not found.");

        if (message.DiscardedAt is not null || message.ReplayedAt is not null)
        {
            throw new ConflictException("DLQ message is no longer active.");
        }

        using var doc = System.Text.Json.JsonDocument.Parse(message.PayloadJson);
        var correlationId = doc.RootElement.TryGetProperty("correlationId", out var cEl) && cEl.TryGetGuid(out var cid)
            ? cid
            : Guid.NewGuid();

        await messageBus.PublishAsync(
            TicketMessaging.IncomingQueueName,
            message.ServiceBusMessageId,
            message.PayloadJson,
            correlationId,
            cancellationToken);

        message.ReplayedAt = clock.UtcNow;
        await dlq.SaveChangesAsync(cancellationToken);
    }

    public async Task DiscardDlqAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await dlq.FindByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("DLQ message was not found.");

        message.DiscardedAt = clock.UtcNow;
        await dlq.SaveChangesAsync(cancellationToken);
    }

    public async Task<OpsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var queue = await reviews.GetQueueAsync(cancellationToken);
        var dlqItems = await dlq.ListActiveAsync(cancellationToken);
        var pending = await outbox.CountPendingAsync(cancellationToken);

        return new OpsSummaryDto(queue.Count, dlqItems.Count, pending);
    }
}