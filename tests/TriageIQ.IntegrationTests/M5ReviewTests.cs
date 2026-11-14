using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Reviews;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;
using TriageIQ.Infrastructure.Persistence;

namespace TriageIQ.IntegrationTests;

public sealed class M5ReviewTests : IClassFixture<TriageIQApiWebApplicationFactory>
{
    private readonly TriageIQApiWebApplicationFactory _factory;

    public M5ReviewTests(TriageIQApiWebApplicationFactory factory)
    {
        _factory = factory;
        factory.EnsureDatabase();
    }

    [Fact]
    public async Task Reviewer_ApprovesNeedsReviewTicket()
    {
        var ticketId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
            db.Users.Add(new User
            {
                Id = reviewerId,
                Email = "reviewer@test.local",
                PasswordHash = "hash",
                Role = UserRole.Reviewer,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            db.Tickets.Add(new Ticket
            {
                Id = ticketId,
                Subject = "needs human",
                Body = "low confidence issue",
                Source = "web",
                Status = TicketStatus.NeedsReview,
                IdempotencyKey = Guid.NewGuid().ToString("N"),
                CorrelationId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var reviews = scope.ServiceProvider.GetRequiredService<ReviewService>();
            var queue = await reviews.GetQueueAsync(CancellationToken.None);
            queue.Any(q => q.TicketId == ticketId).Should().BeTrue();
            await reviews.ApproveAsync(ticketId, reviewerId, CancellationToken.None);
        }

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
            var ticket = await db.Tickets.FindAsync(ticketId);
            ticket!.Status.Should().Be(TicketStatus.ReviewApproved);
            (await db.ReviewActions.CountAsync(a => a.TicketId == ticketId)).Should().Be(1);
        }
    }
}