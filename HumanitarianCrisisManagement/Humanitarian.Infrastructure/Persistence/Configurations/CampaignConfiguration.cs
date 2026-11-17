using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations
{
    public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.ToTable("Campaigns");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Status)
                .HasMaxLength(50);

            // Fix decimal precision warnings for SQL Server
            builder.Property(c => c.TargetAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(c => c.RaisedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00m);

            // Configure Campaign - Donation relationship
            builder.HasMany(c => c.Donations)
                .WithOne(d => d.Campaign)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}