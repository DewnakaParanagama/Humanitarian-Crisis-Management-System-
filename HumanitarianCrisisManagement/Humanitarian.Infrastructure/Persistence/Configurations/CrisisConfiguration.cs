
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class CrisisConfiguration : IEntityTypeConfiguration<Crisis>
{
    public void Configure(EntityTypeBuilder<Crisis> builder)
    {
        builder.ToTable("Crises");

        builder.HasKey(crisis => crisis.Id);

        builder.Property(crisis => crisis.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(crisis => crisis.Type)
            .IsRequired();

        builder.Property(crisis => crisis.Severity)
            .IsRequired();

        builder.Property(crisis => crisis.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(crisis => crisis.AffectedPopulation)
            .IsRequired();

        builder.Property(crisis => crisis.Status)
            .IsRequired();

        builder.Property(crisis => crisis.StartDateUtc)
            .IsRequired();
    }
}

