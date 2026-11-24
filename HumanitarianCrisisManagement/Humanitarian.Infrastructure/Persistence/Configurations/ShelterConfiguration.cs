
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class ShelterConfiguration : IEntityTypeConfiguration<Shelter>
{
    public void Configure(EntityTypeBuilder<Shelter> builder)
    {
        builder.ToTable("Shelters");

        builder.HasKey(shelter => shelter.Id);

        builder.Property(shelter => shelter.Facilities)
            .HasColumnType("nvarchar(max)");

        builder.Property(shelter => shelter.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(shelter => shelter.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(shelter => shelter.Latitude)
    .IsRequired(false);

        builder.Property(shelter => shelter.Longitude)
            .IsRequired(false);

        builder.Property(shelter => shelter.TotalCapacity)
            .IsRequired();

        builder.Property(shelter => shelter.CurrentOccupancy)
            .IsRequired();

        builder.Property(shelter => shelter.Status)
            .IsRequired();

        builder.Property(shelter => shelter.District)
    .IsRequired()
    .HasMaxLength(100);

        builder.Property(shelter => shelter.Contact)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(shelter => shelter.Organization)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(shelter => shelter.Accessible)
            .IsRequired();

        builder.HasOne(shelter => shelter.ShelterAdmin)
            .WithMany()
            .HasForeignKey(shelter => shelter.ShelterAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(shelter => shelter.ShelterAdminId);
    }
}

