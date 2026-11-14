using Microsoft.EntityFrameworkCore;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence;

public sealed class TriageIQDbContext(DbContextOptions<TriageIQDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<PromptVersion> PromptVersions => Set<PromptVersion>();

    public DbSet<TicketAnalysis> TicketAnalyses => Set<TicketAnalysis>();

    public DbSet<LlmCall> LlmCalls => Set<LlmCall>();

    public DbSet<ReviewAction> ReviewActions => Set<ReviewAction>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<EvalRun> EvalRuns => Set<EvalRun>();

    public DbSet<DlqMessage> DlqMessages => Set<DlqMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TriageIQDbContext).Assembly);
    }
}