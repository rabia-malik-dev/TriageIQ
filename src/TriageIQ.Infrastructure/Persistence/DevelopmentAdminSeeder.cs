using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Infrastructure.Persistence;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
        var normalized = email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == normalized, cancellationToken))
        {
            return;
        }

        var hasher = new PasswordHasher<User>();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = normalized,
            PasswordHash = hasher.HashPassword(null!, password),
            Role = UserRole.Admin,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}