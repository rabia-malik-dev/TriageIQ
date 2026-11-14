using TriageIQ.Domain.Enums;

namespace TriageIQ.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}