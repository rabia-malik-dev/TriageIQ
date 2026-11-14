using System.Collections.Concurrent;
using TriageIQ.Application.Abstractions.Messaging;

namespace TriageIQ.Infrastructure.Messaging;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentQueue<PublishedMessage> _messages = new();

    public IReadOnlyCollection<PublishedMessage> Messages => _messages.ToArray();

    public Task PublishAsync(
        string queueName,
        string messageId,
        string bodyJson,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        _messages.Enqueue(new PublishedMessage(queueName, messageId, bodyJson, correlationId));
        return Task.CompletedTask;
    }

    public void Clear() => _messages.Clear();
}

public sealed record PublishedMessage(
    string QueueName,
    string MessageId,
    string BodyJson,
    Guid CorrelationId);