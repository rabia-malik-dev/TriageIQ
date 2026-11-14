using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Messaging;
using TriageIQ.Application.Tickets;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Messaging;

public sealed class TicketIncomingConsumerHostedService(
    IServiceProvider services,
    IOptions<ServiceBusOptions> options,
    ILogger<TicketIncomingConsumerHostedService> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            logger.LogWarning("Service Bus connection string missing; ticket consumer not started");
            return;
        }

        await using var client = new ServiceBusClient(settings.ConnectionString);
        _processor = client.CreateProcessor(settings.IncomingQueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 4,
        });

        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        logger.LogInformation("Listening on queue {Queue}", settings.IncomingQueueName);
        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<TicketReceivedMessage>(
                args.Message.Body.ToString(),
                TicketMessaging.JsonOptions);
            if (payload is null)
            {
                throw new InvalidOperationException("TicketReceived message body was empty or invalid.");
            }

            await using var scope = services.CreateAsyncScope();
            var ingestion = scope.ServiceProvider.GetRequiredService<TicketIngestionService>();
            await ingestion.ProcessTicketReceivedAsync(payload, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process Service Bus message {MessageId} (delivery {DeliveryCount})",
                args.Message.MessageId,
                args.Message.DeliveryCount);

            if (args.Message.DeliveryCount >= 5)
            {
                logger.LogWarning(
                    "Message {MessageId} exceeded max deliveries; Service Bus will dead-letter",
                    args.Message.MessageId);

                await TryRecordDlqAsync(args, ex.Message, args.CancellationToken);
            }

            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Service Bus processor error on {EntityPath}", args.EntityPath);
        return Task.CompletedTask;
    }

    private async Task TryRecordDlqAsync(ProcessMessageEventArgs args, string error, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var dlq = scope.ServiceProvider.GetRequiredService<IDlqRepository>();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();

            await dlq.RecordAsync(
                new DlqMessage
                {
                    Id = Guid.NewGuid(),
                    ServiceBusMessageId = args.Message.MessageId,
                    PayloadJson = args.Message.Body.ToString(),
                    ErrorReason = error,
                    DeliveryCount = (int)args.Message.DeliveryCount,
                    CreatedAt = clock.UtcNow,
                },
                cancellationToken);
        }
        catch (Exception recordEx)
        {
            logger.LogError(recordEx, "Failed to persist DLQ snapshot for message {MessageId}", args.Message.MessageId);
        }
    }
}