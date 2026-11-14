using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Humanitarian.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<Crisis> Crises => Set<Crisis>();

    public DbSet<AssistanceRequest> AssistanceRequests => Set<AssistanceRequest>();

    public DbSet<Shelter> Shelters => Set<Shelter>();

    public DbSet<VolunteerProfile> VolunteerProfiles => Set<VolunteerProfile>();

    public DbSet<VolunteerAssignment> VolunteerAssignments => Set<VolunteerAssignment>();

    public DbSet<Donation> Donations => Set<Donation>();

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Resource> Resources => Set<Resource>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    public Task<IDbContextTransaction> BeginTransactionAsync(
    CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }
}