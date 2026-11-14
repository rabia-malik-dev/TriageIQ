namespace TriageIQ.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; init; } = string.Empty;

    public string Issuer { get; init; } = "TriageIQ";

    public string Audience { get; init; } = "TriageIQ";

    public int ExpiryMinutes { get; init; } = 60;
}