using System.Text.Json;
using Microsoft.Extensions.Options;
using TriageIQ.Application.Abstractions.Llm;
using TriageIQ.Application.Messaging;

namespace TriageIQ.Infrastructure.Llm;

public sealed class FakeLlmClient(IOptions<LlmOptions> options) : ILlmClient
{
    public Task<LlmCompletionResult> CompleteAsync(
        LlmCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var confidence = request.UserPrompt.Contains("urgent", StringComparison.OrdinalIgnoreCase)
            || request.UserPrompt.Contains("critical", StringComparison.OrdinalIgnoreCase)
            ? 0.92m
            : 0.78m;

        var category = request.UserPrompt.Contains("payment", StringComparison.OrdinalIgnoreCase)
            ? "billing"
            : "general";

        var priority = confidence >= 0.9m ? "high" : "medium";

        var payload = JsonSerializer.Serialize(
            new
            {
                category,
                priority,
                summary = $"Automated triage summary for correlation {request.CorrelationId:N}",
                confidence,
            },
            TicketMessaging.JsonOptions);

        var result = new LlmCompletionResult(
            payload,
            Provider: "local",
            Model: settings.Model,
            InputTokens: Math.Max(1, request.SystemPrompt.Length / 4),
            OutputTokens: Math.Max(1, payload.Length / 4),
            LatencyMs: 12,
            EstimatedCostUsd: 0,
            Succeeded: true,
            ErrorMessage: null);

        return Task.FromResult(result);
    }
}