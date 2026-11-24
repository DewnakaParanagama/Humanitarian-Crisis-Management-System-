
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.ToTable("Donations");

        builder.HasKey(donation => donation.Id);

        builder.Property(donation => donation.DonorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(donation => donation.Type)
            .IsRequired();

        builder.Property(donation => donation.AmountOrValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(donation => donation.CampaignName)
            .HasMaxLength(200);

        builder.Property(donation => donation.Status)
            .IsRequired();

        builder.Property(donation => donation.DateReceivedUtc)
            .IsRequired();

        builder.HasIndex(donation => donation.Status);

        builder.HasIndex(donation => donation.DateReceivedUtc);
    }
}
