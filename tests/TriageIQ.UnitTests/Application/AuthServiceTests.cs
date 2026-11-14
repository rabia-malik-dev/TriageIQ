using FluentAssertions;
using NSubstitute;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Abstractions.Security;
using TriageIQ.Application.Auth;
using TriageIQ.Application.Exceptions;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.UnitTests.Application;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        var users = Substitute.For<IUserRepository>();
        users.FindByEmailAsync("a@test.local", Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = Guid.NewGuid(),
                Email = "a@test.local",
                PasswordHash = "hash",
                Role = UserRole.Viewer,
                CreatedAt = DateTimeOffset.UtcNow,
            });

        var service = new AuthService(
            users,
            Substitute.For<IPasswordHasher>(),
            Substitute.For<IJwtTokenGenerator>(),
            Substitute.For<IClock>());

        var action = async () => await service.RegisterAsync(
            new RegisterRequest("a@test.local", "Password123!@#"),
            CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>();
    }
}