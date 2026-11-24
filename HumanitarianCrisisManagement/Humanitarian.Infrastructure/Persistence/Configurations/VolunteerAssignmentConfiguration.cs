using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class VolunteerAssignmentConfiguration
    : IEntityTypeConfiguration<VolunteerAssignment>
{
    public void Configure(
        EntityTypeBuilder<VolunteerAssignment> builder)
    {
        builder.ToTable("VolunteerAssignments");

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.TargetType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(assignment => assignment.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Assigned");

        builder.Property(assignment => assignment.DispatchedAtUtc)
            .IsRequired();

        builder.HasOne(assignment => assignment.VolunteerProfile)
            .WithMany()
            .HasForeignKey(assignment => assignment.VolunteerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(assignment => assignment.VolunteerProfileId);
        builder.HasIndex(assignment => assignment.TargetId);
    }
}