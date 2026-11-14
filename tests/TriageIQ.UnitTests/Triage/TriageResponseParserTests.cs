using FluentAssertions;
using TriageIQ.Application.Triage;

namespace TriageIQ.UnitTests.Triage;

public sealed class TriageResponseParserTests
{
    [Fact]
    public void Parse_ValidJson_ReturnsFields()
    {
        const string json = """
            {
              "category": "billing",
              "priority": "high",
              "summary": "Payment issue",
              "confidence": 0.91
            }
            """;

        var result = TriageResponseParser.Parse(json);

        result.Category.Should().Be("billing");
        result.Priority.Should().Be("high");
        result.Confidence.Should().Be(0.91m);
    }

    [Fact]
    public void Parse_MissingCategory_Throws()
    {
        const string json = """{"priority":"low","summary":"x","confidence":0.5}""";

        var act = () => TriageResponseParser.Parse(json);

        act.Should().Throw<InvalidOperationException>();
    }
}