using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using TriageIQ.Application.Abstractions.Messaging;

namespace TriageIQ.Infrastructure.Messaging;

public sealed class ServiceBusMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusMessageBus(IOptions<ServiceBusOptions> options)
    {
        var settings = options.Value;
        _client = new ServiceBusClient(settings.ConnectionString);
        _sender = _client.CreateSender(settings.IncomingQueueName);
    }

    public Task PublishAsync(
        string queueName,
        string messageId,
        string bodyJson,
        Guid correlationId,
        CancellationToken cancellationToken) =>
        PublishCoreAsync(messageId, bodyJson, correlationId, cancellationToken);

    private async Task PublishCoreAsync(
        string messageId,
        string bodyJson,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var message = new ServiceBusMessage(bodyJson)
        {
            MessageId = messageId,
            CorrelationId = correlationId.ToString(),
            ContentType = "application/json",
        };

        await _sender.SendMessageAsync(message, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}