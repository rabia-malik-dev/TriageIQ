using TriageIQ.Api.Middleware;
using TriageIQ.Api.OpenApi;
using TriageIQ.Application;
using TriageIQ.Infrastructure;
using TriageIQ.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddOutboxPublishing();
}
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddTriageIqSwagger();
builder.Services.AddHttpClient();
builder.Services.AddProblemDetails();

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
if (!builder.Environment.IsEnvironment("Testing") &&
    string.IsNullOrWhiteSpace(jwtSigningKey))
{
    throw new InvalidOperationException("JWT signing key is not configured.");
}

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing")
    && app.Configuration.GetConnectionString("Default") is { Length: > 0 })
{
    await app.Services.ApplyMigrationsAsync(CancellationToken.None);
    await DevelopmentAdminSeeder.SeedAsync(
        app.Services,
        app.Configuration,
        app.Environment,
        CancellationToken.None);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TriageIQ API v1");
        options.RoutePrefix = "swagger";
    });
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;