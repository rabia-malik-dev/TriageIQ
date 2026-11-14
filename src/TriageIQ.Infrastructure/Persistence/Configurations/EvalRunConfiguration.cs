using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Infrastructure.Persistence.Configurations;

public sealed class EvalRunConfiguration : IEntityTypeConfiguration<EvalRun>
{
    public void Configure(EntityTypeBuilder<EvalRun> builder)
    {
        builder.ToTable("EvalRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DatasetName).HasMaxLength(128);
        builder.Property(x => x.Mode).HasMaxLength(32);
        builder.Property(x => x.ReportJson).HasMaxLength(8000);
        builder.Property(x => x.CategoryAccuracy).HasPrecision(8, 4);
        builder.Property(x => x.MacroF1).HasPrecision(8, 4);
        builder.Property(x => x.JsonValidityRate).HasPrecision(8, 4);
        builder.Property(x => x.InjectionCatchRate).HasPrecision(8, 4);
        builder.Property(x => x.AvgCostUsd).HasPrecision(10, 6);
        builder.HasIndex(x => x.CreatedAt);
    }
}