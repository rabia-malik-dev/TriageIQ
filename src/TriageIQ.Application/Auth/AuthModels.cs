using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Auth;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string Email,
    UserRole Role);

public sealed record ChangeUserRoleRequest(UserRole Role);