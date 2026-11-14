using TriageIQ.Application.Triage;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Guardrails;

public sealed record PostTriageGuardrailResult(
    TriageRoutingDecision Routing,
    IReadOnlyList<string> Violations);

public static class PostTriageGuardrails
{
    public static PostTriageGuardrailResult Evaluate(
        TriageLlmResponse response,
        decimal confidenceThreshold,
        decimal maxEstimatedCostUsd,
        decimal estimatedCostUsd)
    {
        var violations = new List<string>();

        try
        {
            TriageOutputValidator.Validate(response);
        }
        catch
        {
            violations.Add(GuardrailCodes.InvalidSchema);
        }

        if (estimatedCostUsd > maxEstimatedCostUsd)
        {
            violations.Add(GuardrailCodes.CostBudgetExceeded);
        }

        if (response.Confidence < confidenceThreshold)
        {
            violations.Add(GuardrailCodes.LowConfidence);
        }

        if (violations.Count > 0)
        {
            return new PostTriageGuardrailResult(TriageRoutingDecision.NeedsReview, violations);
        }

        return new PostTriageGuardrailResult(TriageRoutingDecision.AutoApproved, []);
    }
}