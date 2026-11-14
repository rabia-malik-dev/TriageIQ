using System.Text.Json;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Guardrails;
using TriageIQ.Application.Triage;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Evals;

public sealed class EvalRunnerService(
    IEvalRunRepository evalRuns,
    IClock clock)
{
    public async Task<EvalRunDto> RunAsync(EvalRunRequest request, CancellationToken cancellationToken)
    {
        var datasetPath = Path.Combine(
            AppContext.BaseDirectory,
            "eval",
            "datasets",
            $"{request.DatasetName}.jsonl");

        if (!File.Exists(datasetPath))
        {
            datasetPath = ResolveRepoDatasetPath(request.DatasetName);
        }

        if (!File.Exists(datasetPath))
        {
            throw new FileNotFoundException($"Dataset '{request.DatasetName}' was not found.", datasetPath);
        }

        var gate = EvalQualityGateDefaults.Load();
        var records = await LoadRecordsAsync(datasetPath, cancellationToken);
        var metrics = EvalReplayEngine.Evaluate(records);

        var baseline = EvalQualityGateDefaults.LoadBaseline();
        var passed = gate.IsPassing(metrics, baseline);

        var run = new EvalRun
        {
            Id = Guid.NewGuid(),
            PromptVersionId = request.PromptVersionId,
            DatasetName = request.DatasetName,
            Mode = request.Mode,
            Total = metrics.Total,
            CategoryAccuracy = metrics.CategoryAccuracy,
            MacroF1 = metrics.MacroF1,
            JsonValidityRate = metrics.JsonValidityRate,
            InjectionCatchRate = metrics.InjectionCatchRate,
            AvgCostUsd = metrics.AvgCostUsd,
            P95LatencyMs = metrics.P95LatencyMs,
            Passed = passed,
            ReportJson = JsonSerializer.Serialize(metrics),
            CreatedAt = clock.UtcNow,
        };

        await evalRuns.AddAsync(run, cancellationToken);

        return Map(run);
    }

    public async Task<IReadOnlyList<EvalRunDto>> ListAsync(int take, CancellationToken cancellationToken)
    {
        var runs = await evalRuns.ListRecentAsync(take, cancellationToken);
        return runs.Select(Map).ToList();
    }

    private static EvalRunDto Map(EvalRun run) =>
        new(
            run.Id,
            run.DatasetName,
            run.Mode,
            run.Total,
            run.CategoryAccuracy,
            run.MacroF1,
            run.JsonValidityRate,
            run.InjectionCatchRate,
            run.AvgCostUsd,
            run.P95LatencyMs,
            run.Passed,
            run.CreatedAt);

    private static string ResolveRepoDatasetPath(string datasetName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "eval", "datasets", $"{datasetName}.jsonl");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return Path.Combine("eval", "datasets", $"{datasetName}.jsonl");
    }

    private static async Task<List<EvalDatasetRecord>> LoadRecordsAsync(string path, CancellationToken cancellationToken)
    {
        var list = new List<EvalDatasetRecord>();
        await foreach (var line in File.ReadLinesAsync(path, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var record = JsonSerializer.Deserialize<EvalDatasetRecord>(line, EvalJson.Options);
            if (record is not null)
            {
                list.Add(record);
            }
        }

        return list;
    }

}

public static class EvalQualityGateDefaults
{
    public static EvalQualityGate Load() => new(0.85m, 0.98m, 0.90m, 0.02m);

    public static EvalBaseline LoadBaseline() => new(0.85m, 0.98m, 0.90m);
}

public sealed class EvalDatasetRecord
{
    public required string Subject { get; init; }

    public required string Body { get; init; }

    public required string ExpectedCategory { get; init; }

    public bool IsInjection { get; init; }

    public TriageLlmResponse? RecordedResponse { get; init; }

    public int? RecordedLatencyMs { get; init; }

    public decimal? RecordedCostUsd { get; init; }
}

public static class EvalJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}