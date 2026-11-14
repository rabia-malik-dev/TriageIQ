using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Triage;

public static class TriagePromptBuilder
{
    public const string SystemPrompt =
        """
        You are a support ticket triage assistant. Respond with JSON only using:
        category, priority (low|medium|high|critical), summary, confidence (0-1).
        """;

    public static string BuildUserPrompt(PromptVersion promptVersion, string subject, string redactedBody, string source) =>
        promptVersion.Template
            .Replace("{{subject}}", subject, StringComparison.Ordinal)
            .Replace("{{body}}", redactedBody, StringComparison.Ordinal)
            .Replace("{{source}}", source, StringComparison.Ordinal);
}