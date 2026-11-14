namespace TriageIQ.Application.Evals;

public sealed record EvalRunRequest(
    string DatasetName,
    string Mode,
    Guid? PromptVersionId);

public sealed record EvalRunDto(
    Guid Id,
    string DatasetName,
    string Mode,
    int Total,
    decimal CategoryAccuracy,
    decimal MacroF1,
    decimal JsonValidityRate,
    decimal InjectionCatchRate,
    decimal AvgCostUsd,
    int P95LatencyMs,
    bool Passed,
    DateTimeOffset CreatedAt);

public sealed record EvalMetrics(
    int Total,
    decimal CategoryAccuracy,
    decimal MacroF1,
    decimal JsonValidityRate,
    decimal InjectionCatchRate,
    decimal AvgCostUsd,
    int P95LatencyMs);

public sealed record EvalBaseline(
    decimal CategoryAccuracy,
    decimal JsonValidityRate,
    decimal InjectionCatchRate);

public sealed record EvalQualityGate(
    decimal MinCategoryAccuracy,
    decimal MinJsonValidity,
    decimal MinInjectionCatchRate,
    decimal MaxRegressionPoints)
{
    public bool IsPassing(EvalMetrics metrics, EvalBaseline baseline) =>
        metrics.CategoryAccuracy >= MinCategoryAccuracy
        && metrics.JsonValidityRate >= MinJsonValidity
        && metrics.InjectionCatchRate >= MinInjectionCatchRate
        && baseline.CategoryAccuracy - metrics.CategoryAccuracy <= MaxRegressionPoints
        && baseline.JsonValidityRate - metrics.JsonValidityRate <= MaxRegressionPoints;
}