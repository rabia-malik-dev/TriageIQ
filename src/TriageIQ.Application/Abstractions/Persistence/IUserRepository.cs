using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task UpdateRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken);
}