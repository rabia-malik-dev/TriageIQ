using System.Text.Json;
namespace TriageIQ.Application.Triage;

public static class TriageResponseParser
{
    public static TriageLlmResponse Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("LLM response must be a JSON object.");
        }

        var category = GetRequiredString(root, "category");
        var priority = GetRequiredString(root, "priority");
        var summary = GetRequiredString(root, "summary");
        var confidence = GetRequiredDecimal(root, "confidence");

        if (confidence is < 0 or > 1)
        {
            throw new InvalidOperationException("Confidence must be between 0 and 1.");
        }

        return new TriageLlmResponse(category, priority, summary, confidence);
    }

    private static string GetRequiredString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"LLM response is missing string property '{name}'.");
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"LLM response property '{name}' cannot be empty.")
            : value.Trim();
    }

    private static decimal GetRequiredDecimal(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var property))
        {
            throw new InvalidOperationException($"LLM response is missing property '{name}'.");
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(property.GetString(), out var parsed) => parsed,
            _ => throw new InvalidOperationException($"LLM response property '{name}' must be numeric."),
        };
    }
}