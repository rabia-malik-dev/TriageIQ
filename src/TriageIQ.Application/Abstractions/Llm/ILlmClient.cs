namespace TriageIQ.Application.Abstractions.Llm;

public interface ILlmClient
{
    Task<LlmCompletionResult> CompleteAsync(LlmCompletionRequest request, CancellationToken cancellationToken);
}

public sealed record LlmCompletionRequest(
    string SystemPrompt,
    string UserPrompt,
    Guid CorrelationId);

public sealed record LlmCompletionResult(
    string Content,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    int LatencyMs,
    decimal EstimatedCostUsd,
    bool Succeeded,
    string? ErrorMessage);