using System.Text.Json;
using TriageIQ.Application.Evals;

var mode = args.ElementAtOrDefault(0) ?? "replay";
var datasetName = args.ElementAtOrDefault(1) ?? "triage-v1";

var datasetPath = ResolveDatasetPath(datasetName);
if (!File.Exists(datasetPath))
{
    Console.Error.WriteLine($"Dataset not found: {datasetPath}");
    return 2;
}

var records = new List<EvalDatasetRecord>();
await foreach (var line in File.ReadLinesAsync(datasetPath))
{
    if (string.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    var record = JsonSerializer.Deserialize<EvalDatasetRecord>(line, EvalJson.Options);
    if (record is not null)
    {
        records.Add(record);
    }
}

if (!mode.Equals("live", StringComparison.OrdinalIgnoreCase)
    && !mode.Equals("replay", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Usage: TriageIQ.Evals [replay|live] [datasetName]");
    return 2;
}

var metrics = EvalReplayEngine.Evaluate(records);
var gate = EvalQualityGateDefaults.Load();
var baseline = EvalQualityGateDefaults.LoadBaseline();
var passed = gate.IsPassing(metrics, baseline);

Console.WriteLine($"Mode={mode} Dataset={datasetName} Total={metrics.Total}");
Console.WriteLine($"CategoryAccuracy={metrics.CategoryAccuracy:P2}");
Console.WriteLine($"JsonValidity={metrics.JsonValidityRate:P2}");
Console.WriteLine($"InjectionCatch={metrics.InjectionCatchRate:P2}");
Console.WriteLine($"Quality gate: {(passed ? "PASSED" : "FAILED")}");

return passed ? 0 : 1;

static string ResolveDatasetPath(string datasetName)
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