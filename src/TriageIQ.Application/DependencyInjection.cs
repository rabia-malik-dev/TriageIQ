using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TriageIQ.Application.Auth;
using TriageIQ.Application.Messaging;
using TriageIQ.Application.Tickets;
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
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();

        return services;
    }
}