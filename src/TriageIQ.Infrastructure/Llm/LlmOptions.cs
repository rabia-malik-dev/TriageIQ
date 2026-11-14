namespace TriageIQ.Infrastructure.Llm;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; init; } = "fake";

    public string Model { get; init; } = "fake-triage-v1";

    public int TimeoutSeconds { get; init; } = 30;
}