using TriageIQ.Application.Triage;

namespace TriageIQ.Application.Guardrails;

public static class TriageOutputValidator
{
    private static readonly HashSet<string> AllowedPriorities =
        new(StringComparer.OrdinalIgnoreCase) { "low", "medium", "high", "critical" };

    public static void Validate(TriageLlmResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Category) || response.Category.Length > 64)
        {
            throw new InvalidOperationException("Invalid category in LLM response.");
        }

        if (!AllowedPriorities.Contains(response.Priority))
        {
            throw new InvalidOperationException("Invalid priority in LLM response.");
        }

        if (string.IsNullOrWhiteSpace(response.Summary) || response.Summary.Length > 2000)
        {
            throw new InvalidOperationException("Invalid summary in LLM response.");
        }
    }
}