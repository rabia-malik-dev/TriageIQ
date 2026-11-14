using Microsoft.AspNetCore.Identity; // PasswordHasher<TUser>
using TriageIQ.Application.Abstractions.Security;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string password) =>
        _hasher.HashPassword(null!, password);

    public bool VerifyPassword(string hashedPassword, string providedPassword) =>
        _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword)
            is not PasswordVerificationResult.Failed;
}