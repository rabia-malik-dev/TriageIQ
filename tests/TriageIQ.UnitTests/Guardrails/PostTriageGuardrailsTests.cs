using FluentAssertions;
using TriageIQ.Application.Guardrails;
using TriageIQ.Application.Triage;
using TriageIQ.Domain.Enums;

namespace TriageIQ.UnitTests.Guardrails;

public sealed class PostTriageGuardrailsTests
{
    [Fact]
    public void Evaluate_AutoApprovesWhenWithinThresholdAndBudget()
    {
        var response = new TriageLlmResponse("billing", "medium", "ok", 0.9m);

        var result = PostTriageGuardrails.Evaluate(response, 0.85m, 0.25m, 0.01m);

        result.Routing.Should().Be(TriageRoutingDecision.AutoApproved);
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_NeedsReviewWhenConfidenceLow()
    {
        var response = new TriageLlmResponse("billing", "medium", "uncertain", 0.5m);

        var result = PostTriageGuardrails.Evaluate(response, 0.85m, 0.25m, 0.01m);

        result.Routing.Should().Be(TriageRoutingDecision.NeedsReview);
        result.Violations.Should().Contain(GuardrailCodes.LowConfidence);
    }

    [Fact]
    public void Evaluate_NeedsReviewWhenCostExceedsBudget()
    {
        var response = new TriageLlmResponse("billing", "medium", "ok", 0.95m);

        var result = PostTriageGuardrails.Evaluate(response, 0.85m, 0.10m, 0.50m);

        result.Routing.Should().Be(TriageRoutingDecision.NeedsReview);
        result.Violations.Should().Contain(GuardrailCodes.CostBudgetExceeded);
    }
}