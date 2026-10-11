namespace Humanitarian.Application.Common.Interfaces;

public interface IAppDbContext
{
    // DbSet properties will be added here as each module creates its entities.
    // Example (later): DbSet<CrisisEvent> CrisisEvents { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}