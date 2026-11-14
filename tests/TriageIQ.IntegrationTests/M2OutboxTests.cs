using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TriageIQ.Application.Messaging;
using TriageIQ.Domain.Enums;
using TriageIQ.Infrastructure.Messaging;
using TriageIQ.Infrastructure.Persistence;

namespace TriageIQ.IntegrationTests;

public sealed class M2OutboxTests : IClassFixture<TriageIQApiWebApplicationFactory>
{
    private readonly TriageIQApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public M2OutboxTests(TriageIQApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.EnsureDatabase();
    }

    [Fact]
    public async Task CreateTicket_WritesPendingOutboxRow()
    {
        var token = await LoginAdminAsync();
        var idempotencyKey = Guid.NewGuid().ToString("N");

        var response = await SendCreateTicketAsync(token, idempotencyKey);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();

        var outbox = (await db.OutboxMessages
                .Where(o => o.MessageType == TicketMessaging.TicketReceivedType)
                .ToListAsync())
            .OrderByDescending(o => o.CreatedAt)
            .First();

        outbox.MessageType.Should().Be(TicketMessaging.TicketReceivedType);
        outbox.Status.Should().Be(OutboxMessageStatus.Pending);
        outbox.PayloadJson.Should().Contain("ticketId");
    }

    [Fact]
    public async Task OutboxDispatcher_PublishesToInMemoryBus_OncePerNewTicket()
    {
        var token = await LoginAdminAsync();
        var idempotencyKey = Guid.NewGuid().ToString("N");

        await SendCreateTicketAsync(token, idempotencyKey);

        var bus = _factory.Services.GetRequiredService<InMemoryMessageBus>();
        bus.Clear();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
            var published = await dispatcher.DispatchPendingAsync(CancellationToken.None);
            published.Should().Be(1);
        }

        bus.Messages.Should().HaveCount(1);
        bus.Messages.First().BodyJson.Should().Contain("ticketId");

        await SendCreateTicketAsync(token, idempotencyKey);
        bus.Clear();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
            var published = await dispatcher.DispatchPendingAsync(CancellationToken.None);
            published.Should().Be(0);
        }

        bus.Messages.Should().BeEmpty();
    }

    private async Task<string> LoginAdminAsync()
    {
        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = TriageIQApiWebApplicationFactory.AdminEmail, password = TriageIQApiWebApplicationFactory.AdminPassword });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthLoginResponse>();
        return auth!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendCreateTicketAsync(string token, string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tickets");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = JsonContent.Create(new
        {
            subject = "Outbox integration ticket",
            body = "M2 messaging test body",
            source = "integration-test",
        });

        return await _client.SendAsync(request);
    }

    private sealed record AuthLoginResponse(string AccessToken);
}