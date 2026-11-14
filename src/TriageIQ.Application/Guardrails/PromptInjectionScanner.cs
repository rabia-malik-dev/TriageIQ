namespace TriageIQ.Application.Guardrails;

public static class PromptInjectionScanner
{
    private static readonly string[] SuspiciousPhrases =
    [
        "ignore previous instructions",
        "ignore all prior",
        "disregard your instructions",
        "you are now",
        "system prompt",
        "reveal your instructions",
        "jailbreak",
        "<|im_start|>",
        "override safety",
    ];

    public static bool ContainsSuspiciousContent(string subject, string body)
    {
        var combined = $"{subject}\n{body}";
        foreach (var phrase in SuspiciousPhrases)
        {
            if (combined.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}