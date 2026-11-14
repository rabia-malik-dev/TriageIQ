using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Abstractions.Security;
using TriageIQ.Application.Exceptions;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IClock clock)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await users.FindByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = UserRole.Viewer,
            CreatedAt = clock.UtcNow,
        };

        await users.AddAsync(user, cancellationToken);
        await users.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await users.FindByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return BuildAuthResponse(user);
    }

    public async Task ChangeUserRoleAsync(Guid userId, UserRole newRole, CancellationToken cancellationToken)
    {
        if (await users.FindByIdAsync(userId, cancellationToken) is null)
        {
            throw new NotFoundException("User was not found.");
        }

        await users.UpdateRoleAsync(userId, newRole, cancellationToken);
        await users.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expiresAt) = tokenGenerator.CreateToken(user);
        return new AuthResponse(token, expiresAt, user.Email, user.Role);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}