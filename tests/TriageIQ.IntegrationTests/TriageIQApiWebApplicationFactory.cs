using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;
using TriageIQ.Infrastructure.Persistence;

namespace TriageIQ.IntegrationTests;

public sealed class TriageIQApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly object _initLock = new();
    private bool _initialized;
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Password123!@#";

    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    private static string TestJwtSigningKey() => new('t', 48);

    public void EnsureDatabase()
    {
        if (_initialized)
        {
            return;
        }

        lock (_initLock)
        {
            if (_initialized)
            {
                return;
            }

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            if (!db.Users.Any(u => u.Email == AdminEmail))
            {
                var hasher = new PasswordHasher<User>();
                db.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = AdminEmail,
                    PasswordHash = hasher.HashPassword(null!, AdminPassword),
                    Role = UserRole.Admin,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
                db.SaveChanges();
            }

            _initialized = true;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var sqlite = $"DataSource=file:{_databaseName}?mode=memory&cache=shared";
        builder.UseSetting("ConnectionStrings:Default", sqlite);
        builder.UseSetting("Jwt:SigningKey", TestJwtSigningKey());
        builder.UseSetting("Jwt:Issuer", "TriageIQ");
        builder.UseSetting("Jwt:Audience", "TriageIQ");
        builder.UseSetting("ServiceBus:EmulatorHealthUrl", string.Empty);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = sqlite,
                ["Jwt:SigningKey"] = TestJwtSigningKey(),
                ["Jwt:Issuer"] = "TriageIQ",
                ["Jwt:Audience"] = "TriageIQ",
                ["ServiceBus:EmulatorHealthUrl"] = string.Empty,
                ["Llm:Provider"] = "local",
                ["Llm:Model"] = "triage-local-v1",
                ["Triage:ConfidenceThreshold"] = "0.85",
            });
        });
    }
}