using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using TriageIQ.Infrastructure.Health;

namespace TriageIQ.UnitTests.Infrastructure;

public sealed class ServiceBusEmulatorHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenHealthUrlNotConfigured_ReturnsHealthy()
    {
        var configuration = new ConfigurationBuilder().Build();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        var check = new ServiceBusEmulatorHealthCheck(configuration, httpClientFactory);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }
}