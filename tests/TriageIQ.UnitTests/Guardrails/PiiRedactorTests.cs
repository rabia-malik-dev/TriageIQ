using FluentAssertions;
using TriageIQ.Application.Guardrails;

namespace TriageIQ.UnitTests.Guardrails;

public sealed class PiiRedactorTests
{
    [Fact]
    public void Redact_ReplacesEmailPhoneSsnAndCard()
    {
        var input = "Reach me at alice@corp.com or 555-123-4567. SSN 123-45-6789. Card 4111 1111 1111 1111";

        var result = PiiRedactor.Redact(input);

        result.Should().Contain("[REDACTED_EMAIL]");
        result.Should().Contain("[REDACTED_PHONE]");
        result.Should().Contain("[REDACTED_SSN]");
        result.Should().Contain("[REDACTED_CARD]");
        result.Should().NotContain("alice@corp.com");
    }
}