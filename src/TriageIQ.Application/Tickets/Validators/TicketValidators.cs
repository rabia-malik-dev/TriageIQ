using FluentValidation;

namespace TriageIQ.Application.Tickets.Validators;

public sealed class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(TicketLimits.MaxSubjectLength);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(TicketLimits.MaxBodyLength);

        RuleFor(x => x.Source)
            .NotEmpty()
            .MaximumLength(TicketLimits.MaxSourceLength);
    }
}

public sealed class BulkCreateTicketsRequestValidator : AbstractValidator<BulkCreateTicketsRequest>
{
    public BulkCreateTicketsRequestValidator()
    {
        RuleFor(x => x.Tickets)
            .NotEmpty()
            .Must(tickets => tickets.Count <= TicketLimits.MaxBulkCount)
            .WithMessage($"Bulk requests support at most {TicketLimits.MaxBulkCount} tickets.");

        RuleForEach(x => x.Tickets).ChildRules(item =>
        {
            item.RuleFor(t => t.IdempotencyKey).NotEmpty().MaximumLength(128);
            item.RuleFor(t => t.Subject).NotEmpty().MaximumLength(TicketLimits.MaxSubjectLength);
            item.RuleFor(t => t.Body).NotEmpty().MaximumLength(TicketLimits.MaxBodyLength);
            item.RuleFor(t => t.Source).NotEmpty().MaximumLength(TicketLimits.MaxSourceLength);
        });
    }
}