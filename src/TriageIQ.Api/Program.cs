using TriageIQ.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var includeExternalDependencies = !builder.Environment.IsEnvironment("Testing");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddInfrastructure(builder.Configuration, includeExternalDependencies);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

public partial class Program;