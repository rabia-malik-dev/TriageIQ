using Microsoft.EntityFrameworkCore;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence;

public sealed class TriageIQDbContext(DbContextOptions<TriageIQDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TriageIQDbContext).Assembly);
    }
}