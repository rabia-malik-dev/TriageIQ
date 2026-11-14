namespace TriageIQ.Application.Guardrails;

public static class GuardrailCodes
{
    public const string PromptInjection = "prompt_injection";
    public const string InvalidSchema = "invalid_schema";
    public const string LowConfidence = "low_confidence";
    public const string CostBudgetExceeded = "cost_budget_exceeded";
    public const string LlmFailed = "llm_failed";
}