using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class VolunteerProfileConfiguration
    : IEntityTypeConfiguration<VolunteerProfile>
{
    public void Configure(EntityTypeBuilder<VolunteerProfile> builder)
    {
        builder.ToTable("VolunteerProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Skills)
            .HasColumnType("nvarchar(max)");

        builder.Property(profile => profile.Availability)
            .IsRequired();

        builder.Property(profile => profile.AvailabilityDetails)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(profile => profile.HasVehicle)
            .IsRequired();

        builder.Property(profile => profile.VehicleType)
            .HasMaxLength(100);

        builder.Property(profile => profile.VerificationStatus)
            .IsRequired();

        builder.Property(profile => profile.TasksCompleted)
            .IsRequired();

        builder.Property(profile => profile.CurrentLocation)
            .HasMaxLength(300);

        builder.HasOne(profile => profile.User)
            .WithOne()
            .HasForeignKey<VolunteerProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();
    }
}