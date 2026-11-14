using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class ReviewActionConfiguration : IEntityTypeConfiguration<ReviewAction>
{
    public void Configure(EntityTypeBuilder<ReviewAction> builder)
    {
        builder.ToTable("ReviewActions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EditedCategory).HasMaxLength(64);
        builder.Property(x => x.EditedPriority).HasMaxLength(32);
        builder.Property(x => x.EditedReply).HasMaxLength(2000);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.ReviewerId);
        builder.HasIndex(x => x.CreatedAt);
    }
}