using Microsoft.EntityFrameworkCore;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(TriageIQDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await db.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task UpdateRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        user.Role = role;
    }
}