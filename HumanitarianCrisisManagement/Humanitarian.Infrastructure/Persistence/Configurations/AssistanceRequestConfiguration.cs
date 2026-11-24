
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class AssistanceRequestConfiguration
    : IEntityTypeConfiguration<AssistanceRequest>
{
    public void Configure(EntityTypeBuilder<AssistanceRequest> builder)
    {
        builder.ToTable("AssistanceRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(request => request.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(request => request.Category)
            .IsRequired();

        builder.Property(request => request.PriorityUrgency)
            .IsRequired();

        builder.Property(request => request.Status)
            .IsRequired();

        builder.Property(request => request.AIRecommendation)
            .HasMaxLength(2000);

        builder.Property(request => request.AIConfidenceScore)
            .HasPrecision(5, 4);

        builder.HasOne(request => request.Citizen)
            .WithMany()
            .HasForeignKey(request => request.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.Crisis)
            .WithMany()
            .HasForeignKey(request => request.CrisisId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => request.CitizenId);

        builder.HasIndex(request => request.CrisisId);

        builder.HasIndex(request => request.Status);
    }
}

