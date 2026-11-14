using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TriageIQ.Application.Messaging;

namespace TriageIQ.Infrastructure.Messaging;

public sealed class OutboxDispatchHostedService(
    IServiceProvider services,
    ILogger<OutboxDispatchHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox dispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
                var published = await dispatcher.DispatchPendingAsync(stoppingToken);

                if (published > 0)
                {
                    logger.LogInformation("Published {Count} outbox message(s)", published);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Outbox dispatch loop failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}