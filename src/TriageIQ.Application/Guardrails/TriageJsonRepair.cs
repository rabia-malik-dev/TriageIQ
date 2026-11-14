using System.Text.RegularExpressions;

namespace TriageIQ.Application.Guardrails;

public static partial class TriageJsonRepair
{
    [GeneratedRegex(@"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex FencedJsonPattern();

    public static string? TryExtractJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var trimmed = content.Trim();
        var fenceMatch = FencedJsonPattern().Match(trimmed);
        if (fenceMatch.Success)
        {
            return fenceMatch.Groups[1].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return null;
    }
}