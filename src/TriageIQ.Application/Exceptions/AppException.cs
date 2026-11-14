namespace TriageIQ.Application.Exceptions;

public abstract class AppException(string message) : Exception(message);

public sealed class ConflictException(string message) : AppException(message);

public sealed class UnauthorizedException(string message) : AppException(message);

public sealed class NotFoundException(string message) : AppException(message);

public sealed class ValidationFailureException(IReadOnlyDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}