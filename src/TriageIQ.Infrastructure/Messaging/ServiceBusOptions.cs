namespace TriageIQ.Infrastructure.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; init; } = string.Empty;

    public string IncomingQueueName { get; init; } = "tickets.incoming";
}