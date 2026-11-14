using FluentAssertions;
using TriageIQ.Application.Guardrails;
using TriageIQ.Application.Triage;

namespace TriageIQ.UnitTests.Guardrails;

public sealed class TriageOutputValidatorTests
{
    [Fact]
    public void Validate_AcceptsAllowedPriority()
    {
        var response = new TriageLlmResponse("billing", "high", "Payment issue", 0.9m);

        var act = () => TriageOutputValidator.Validate(response);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_RejectsUnknownPriority()
    {
        var response = new TriageLlmResponse("billing", "urgent", "Payment issue", 0.9m);

        var act = () => TriageOutputValidator.Validate(response);

        act.Should().Throw<InvalidOperationException>();
    }
}