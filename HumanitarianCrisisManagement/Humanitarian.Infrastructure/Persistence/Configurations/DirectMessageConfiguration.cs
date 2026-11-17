using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class DirectMessageConfiguration : IEntityTypeConfiguration<DirectMessage>
{
    public void Configure(EntityTypeBuilder<DirectMessage> builder)
    {
        builder.HasKey(dm => dm.Id);

        builder.Property(dm => dm.MessageBody)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(dm => dm.Request)
            .WithMany()
            .HasForeignKey(dm => dm.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}