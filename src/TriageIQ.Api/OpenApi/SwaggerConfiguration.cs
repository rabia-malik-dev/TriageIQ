using Microsoft.OpenApi;

namespace TriageIQ.Api.OpenApi;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddTriageIqSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TriageIQ API",
                Version = "v1",
                Description = "Event-driven ticket triage API (M1: auth and tickets).",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT bearer token. Example: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });
        });

        return services;
    }
}