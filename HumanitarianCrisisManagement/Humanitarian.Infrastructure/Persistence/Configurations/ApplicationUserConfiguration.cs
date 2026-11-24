
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Humanitarian.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(user => user.Role)
            .IsRequired();

        builder.Property(user => user.Status)
            .IsRequired();
    }
}

