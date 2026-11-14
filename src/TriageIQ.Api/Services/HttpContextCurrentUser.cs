using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TriageIQ.Application.Abstractions;

namespace TriageIQ.Api.Services;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public bool IsInRole(string role) =>
        accessor.HttpContext?.User.IsInRole(role) ?? false;
}