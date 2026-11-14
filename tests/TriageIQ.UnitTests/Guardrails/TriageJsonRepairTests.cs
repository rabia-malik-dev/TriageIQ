using FluentAssertions;
using TriageIQ.Application.Guardrails;

namespace TriageIQ.UnitTests.Guardrails;

public sealed class TriageJsonRepairTests
{
    [Fact]
    public void TryExtractJson_ParsesFencedBlock()
    {
        var content = """
            Here is the triage result:
            ```json
            {"category":"billing","priority":"medium","summary":"ok","confidence":0.8}
            ```
            """;

        var json = TriageJsonRepair.TryExtractJson(content);

        json.Should().Contain("billing");
    }

    [Fact]
    public void TryExtractJson_ParsesEmbeddedObject()
    {
        var content = "Noise before {\"category\":\"x\",\"priority\":\"low\",\"summary\":\"s\",\"confidence\":0.5} noise after";

        var json = TriageJsonRepair.TryExtractJson(content);

        json.Should().StartWith("{").And.EndWith("}");
    }
}