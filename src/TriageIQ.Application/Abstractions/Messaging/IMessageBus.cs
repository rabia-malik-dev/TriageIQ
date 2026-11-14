namespace TriageIQ.Application.Abstractions.Messaging;

public interface IMessageBus
{
    Task PublishAsync(
        string queueName,
        string messageId,
        string bodyJson,
        Guid correlationId,
        CancellationToken cancellationToken);
}