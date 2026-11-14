namespace TriageIQ.Application.Guardrails;

public static class PreTriageGuardrails
{
    public static GuardrailEvaluation Evaluate(string subject, string body)
    {
        if (PromptInjectionScanner.ContainsSuspiciousContent(subject, body))
        {
            return GuardrailEvaluation.Blocked([GuardrailCodes.PromptInjection]);
        }

        return GuardrailEvaluation.Pass;
    }
}