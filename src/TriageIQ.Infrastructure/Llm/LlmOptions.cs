namespace TriageIQ.Infrastructure.Llm;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; init; } = "local";

    public string Model { get; init; } = "triage-local-v1";

    public int TimeoutSeconds { get; init; } = 30;
}