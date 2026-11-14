using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Subject).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Body).IsRequired();
        builder.Property(t => t.Source).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(t => t.IdempotencyKey).HasMaxLength(128).IsRequired();
        builder.HasIndex(t => t.IdempotencyKey).IsUnique();
        builder.Property(t => t.CorrelationId).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
    }
}