using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Tickets;
using TriageIQ.Application.Triage;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;
using TriageIQ.Infrastructure.Persistence;

namespace TriageIQ.IntegrationTests;

public sealed class M3TriageTests : IClassFixture<TriageIQApiWebApplicationFactory>
{
    private readonly TriageIQApiWebApplicationFactory _factory;

    public M3TriageTests(TriageIQApiWebApplicationFactory factory)
    {
        _factory = factory;
        factory.EnsureDatabase();
    }

    [Fact]
    public async Task TicketTriage_CreatesAnalysisAndLlmCall_WithFakeClient()
    {
        var ticketId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var prompts = scope.ServiceProvider.GetRequiredService<IPromptVersionRepository>();
            await prompts.EnsureDefaultSeedAsync(CancellationToken.None);

            var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
            db.Tickets.Add(new Ticket
            {
                Id = ticketId,
                Subject = "urgent payment failure",
                Body = "Card declined for user test@example.com",
                Source = "email",
                Status = TicketStatus.QueuedForTriage,
                IdempotencyKey = Guid.NewGuid().ToString("N"),
                CorrelationId = correlationId,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var triage = scope.ServiceProvider.GetRequiredService<TicketTriageService>();
            await triage.RunAsync(ticketId, correlationId, CancellationToken.None);
        }

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
            var ticket = await db.Tickets.FindAsync(ticketId);
            ticket!.Status.Should().Be(TicketStatus.AutoApproved);
            ticket.BodyRedacted.Should().Contain("[REDACTED_EMAIL]");

            var analyses = await db.TicketAnalyses.Where(a => a.TicketId == ticketId).ToListAsync();
            analyses.Should().HaveCount(1);
            analyses[0].Category.Should().Be("billing");
            analyses[0].Confidence.Should().BeGreaterThan(0.85m);

            var calls = await db.LlmCalls.Where(c => c.TicketId == ticketId).ToListAsync();
            calls.Should().HaveCount(1);
            calls[0].Provider.Should().Be("fake");
            calls[0].Succeeded.Should().BeTrue();
        }
    }
}