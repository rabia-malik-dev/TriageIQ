using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(User user);
}