using System.Text.RegularExpressions;

namespace TriageIQ.Application.Triage;

public static partial class PiiRedactor
{
    [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex PhonePattern();

    public static string Redact(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var redacted = EmailPattern().Replace(input, "[REDACTED_EMAIL]");
        redacted = PhonePattern().Replace(redacted, "[REDACTED_PHONE]");
        return redacted;
    }
}