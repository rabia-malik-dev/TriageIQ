using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TriageIQ.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<TriageIQWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(TriageIQWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_ReturnsOk()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Ready_InTestingEnvironment_ReturnsOk()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}