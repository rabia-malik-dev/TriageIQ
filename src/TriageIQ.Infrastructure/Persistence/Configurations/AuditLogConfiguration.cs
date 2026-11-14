using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(128);
        builder.Property(x => x.Entity).HasMaxLength(64);
        builder.Property(x => x.EntityId).HasMaxLength(64);
        builder.Property(x => x.Details).HasMaxLength(4000);
        builder.HasIndex(x => x.CreatedAt);
    }
}