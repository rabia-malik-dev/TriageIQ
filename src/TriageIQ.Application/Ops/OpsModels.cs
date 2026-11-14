namespace TriageIQ.Application.Ops;

public sealed record OpsHealthDto(
    string Api,
    string Worker,
    string Database,
    string ServiceBus);

public sealed record DlqMessageDto(
    Guid Id,
    string ServiceBusMessageId,
    string? ErrorReason,
    int DeliveryCount,
    DateTimeOffset CreatedAt);

public sealed record OpsSummaryDto(
    int ReviewQueueSize,
    int DlqCount,
    int PendingOutbox);