namespace TriageIQ.Application.Guardrails;

public sealed record GuardrailEvaluation(
    bool AllowLlmCall,
    bool ForceNeedsReview,
    IReadOnlyList<string> Violations)
{
    public static GuardrailEvaluation Pass { get; } = new(true, false, []);

    public static GuardrailEvaluation Blocked(IReadOnlyList<string> violations, bool needsReview = true) =>
        new(false, needsReview, violations);
}