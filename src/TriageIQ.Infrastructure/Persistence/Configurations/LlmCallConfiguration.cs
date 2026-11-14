using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class LlmCallConfiguration : IEntityTypeConfiguration<LlmCall>
{
    public void Configure(EntityTypeBuilder<LlmCall> builder)
    {
        builder.ToTable("LlmCalls");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Provider).HasMaxLength(64).IsRequired();
        builder.Property(c => c.Model).HasMaxLength(128).IsRequired();
        builder.Property(c => c.EstimatedCostUsd).HasPrecision(12, 6);
        builder.Property(c => c.ErrorMessage).HasMaxLength(2048);
        builder.HasIndex(c => c.TicketId);
        builder.HasIndex(c => c.CorrelationId);
    }
}