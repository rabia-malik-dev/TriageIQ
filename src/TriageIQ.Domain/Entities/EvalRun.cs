namespace TriageIQ.Domain.Entities;

public sealed class EvalRun
{
    public Guid Id { get; set; }

    public Guid? PromptVersionId { get; set; }

    public required string DatasetName { get; set; }

    public required string Mode { get; set; }

    public int Total { get; set; }

    public decimal CategoryAccuracy { get; set; }

    public decimal MacroF1 { get; set; }

    public decimal JsonValidityRate { get; set; }

    public decimal InjectionCatchRate { get; set; }

    public decimal AvgCostUsd { get; set; }

    public int P95LatencyMs { get; set; }

    public bool Passed { get; set; }

    public string? ReportJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}