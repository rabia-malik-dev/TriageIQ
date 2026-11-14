using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TriageIQ.Application.Auth;
using TriageIQ.Application.Messaging;
using TriageIQ.Application.Tickets;
using TriageIQ.Application.Analytics;
using TriageIQ.Application.Evals;
using TriageIQ.Application.Ops;
using TriageIQ.Application.Prompts;
using TriageIQ.Application.Reviews;
using TriageIQ.Application.Triage;
using TriageIQ.Application.Validation;

namespace TriageIQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AuthService>();
        services.AddScoped<ValidationService>();
        services.AddScoped<AuthService>();
        services.AddScoped<TicketService>();
        services.AddScoped<TicketIngestionService>();
        services.AddScoped<TicketTriageService>();
        services.AddScoped<TicketAnalysisQueryService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<PromptManagementService>();
        services.AddScoped<EvalRunnerService>();
        services.AddScoped<AnalyticsQueryService>();
        services.AddScoped<OpsService>();
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();

        return services;
    }
}