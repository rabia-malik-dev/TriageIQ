using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.MessageType).HasMaxLength(64).IsRequired();
        builder.Property(o => o.PayloadJson).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(o => o.LastError).HasMaxLength(2048);

        builder.HasIndex(o => new { o.Status, o.CreatedAt });
    }
}