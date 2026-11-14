using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Abstractions.Security;
using TriageIQ.Infrastructure.Health;
using TriageIQ.Infrastructure.Persistence;
using TriageIQ.Infrastructure.Persistence.Repositories;
using TriageIQ.Infrastructure.Security;
using TriageIQ.Infrastructure.Time;

namespace TriageIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Default");
        var hasDatabase = !string.IsNullOrWhiteSpace(connectionString);

        if (hasDatabase)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            if (environment.IsEnvironment("Testing"))
            {
                services.AddDbContext<TriageIQDbContext>(options => options.UseSqlite(connectionString));
            }
            else
            {
                services.AddDbContext<TriageIQDbContext>(options => options.UseSqlServer(connectionString));
            }

            AddAuthentication(services, configuration, environment);
        }

        AddHealthChecks(services, configuration, environment);

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TriageIQDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    private static void AddAuthentication(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
        {
            if (!environment.IsEnvironment("Testing"))
            {
                throw new InvalidOperationException("JWT signing key is not configured.");
            }

            return;
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };
            });

        services.AddAuthorization();
    }

    private static void AddHealthChecks(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var includeExternalDependencies = !environment.IsEnvironment("Testing");

        var healthChecks = services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live", "ready"]);

        if (!includeExternalDependencies)
        {
            return;
        }

        var sqlHealthConnectionString =
            configuration.GetConnectionString("Health")
            ?? configuration.GetConnectionString("Default");

        if (!string.IsNullOrWhiteSpace(sqlHealthConnectionString))
        {
            healthChecks.AddSqlServer(
                sqlHealthConnectionString,
                name: "sqlserver",
                tags: ["ready"]);
        }

        healthChecks.AddCheck<ServiceBusEmulatorHealthCheck>(
            "servicebus",
            tags: ["ready"]);
    }
}