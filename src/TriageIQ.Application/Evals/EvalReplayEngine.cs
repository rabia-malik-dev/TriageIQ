using TriageIQ.Application.Guardrails;
using TriageIQ.Application.Triage;

namespace TriageIQ.Application.Evals;

public static class EvalReplayEngine
{
    public static EvalMetrics Evaluate(IReadOnlyList<EvalDatasetRecord> records)
    {
        var total = records.Count;
        var categoryHits = 0;
        var jsonValid = 0;
        var injectionCaught = 0;
        var injectionTotal = 0;
        var latencies = new List<int>(total);
        var costs = new List<decimal>(total);

        foreach (var record in records)
        {
            var pre = PreTriageGuardrails.Evaluate(record.Subject, record.Body);
            if (record.IsInjection)
            {
                injectionTotal++;
                if (!pre.AllowLlmCall)
                {
                    injectionCaught++;
                }
            }

            if (pre.AllowLlmCall && record.RecordedResponse is not null)
            {
                try
                {
                    TriageOutputValidator.Validate(record.RecordedResponse);
                    jsonValid++;
                    if (string.Equals(
                            record.RecordedResponse.Category,
                            record.ExpectedCategory,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        categoryHits++;
                    }
                }
                catch
                {
                    // invalid
                }

                latencies.Add(record.RecordedLatencyMs ?? 120);
                costs.Add(record.RecordedCostUsd ?? 0.001m);
            }
            else if (!pre.AllowLlmCall && record.IsInjection)
            {
                jsonValid++;
            }
        }

        return new EvalMetrics(
            total,
            SafeRate(categoryHits, total),
            SafeRate(categoryHits, total),
            SafeRate(jsonValid, total),
            SafeRate(injectionCaught, Math.Max(injectionTotal, 1)),
            costs.Count == 0 ? 0 : costs.Average(),
            Percentile(latencies, 0.95));
    }

    private static decimal SafeRate(int hits, int total) => total == 0 ? 0 : Math.Round((decimal)hits / total, 4);

    private static int Percentile(IReadOnlyList<int> values, double quantile)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        var sorted = values.OrderBy(v => v).ToList();
        var index = (int)Math.Ceiling(quantile * sorted.Count) - 1;
        index = Math.Clamp(index, 0, sorted.Count - 1);
        return sorted[index];
    }
}