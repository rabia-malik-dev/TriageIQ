using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class DlqMessageConfiguration : IEntityTypeConfiguration<DlqMessage>
{
    public void Configure(EntityTypeBuilder<DlqMessage> builder)
    {
        builder.ToTable("DlqMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServiceBusMessageId).HasMaxLength(128);
        builder.Property(x => x.PayloadJson).HasMaxLength(8000);
        builder.Property(x => x.ErrorReason).HasMaxLength(2000);
        builder.HasIndex(x => x.CreatedAt);
    }
}