using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class TicketAnalysisConfiguration : IEntityTypeConfiguration<TicketAnalysis>
{
    public void Configure(EntityTypeBuilder<TicketAnalysis> builder)
    {
        builder.ToTable("TicketAnalyses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Category).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Priority).HasMaxLength(32).IsRequired();
        builder.Property(a => a.Summary).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.Confidence).HasPrecision(5, 4);
        builder.Property(a => a.RoutingDecision).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.RawResponseJson).IsRequired();
        builder.Property(a => a.ReviewReason).HasMaxLength(512);
        builder.HasIndex(a => a.TicketId);
        builder.HasOne(a => a.Ticket).WithMany().HasForeignKey(a => a.TicketId);
    }
}