
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(log => log.EntityType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(log => log.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(log => log.PreviousValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(log => log.NewValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(log => log.IpAddress)
            .HasMaxLength(45);

        builder.Property(log => log.TimestampUtc)
            .IsRequired();

        builder.HasOne(log => log.User)
            .WithMany()
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(log => log.UserId);

        builder.HasIndex(log => log.TimestampUtc);

        builder.HasIndex(log => new
        {
            log.EntityType,
            log.EntityId
        });
    }
}

