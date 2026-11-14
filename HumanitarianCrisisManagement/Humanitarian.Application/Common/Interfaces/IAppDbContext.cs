using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Humanitarian.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }

    DbSet<Crisis> Crises { get; }

    DbSet<AssistanceRequest> AssistanceRequests { get; }

    DbSet<Shelter> Shelters { get; }

    DbSet<VolunteerProfile> VolunteerProfiles { get; }

    DbSet<VolunteerAssignment> VolunteerAssignments { get; }

    DbSet<Donation> Donations { get; }

    DbSet<Campaign> Campaigns { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<Resource> Resources { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<DirectMessage> DirectMessages { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}