using FluentValidation;
using TriageIQ.Application.Exceptions;

namespace TriageIQ.Application.Validation;

public sealed class ValidationService(IServiceProvider serviceProvider)
{
    public async Task ValidateAndThrowAsync<T>(T instance, CancellationToken cancellationToken)
    {
        var validator = serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>;
        if (validator is null)
        {
            return;
        }

        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        throw new ValidationFailureException(errors);
    }
}