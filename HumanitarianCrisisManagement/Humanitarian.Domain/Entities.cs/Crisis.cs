using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class Crisis : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public CrisisType Type { get; set; }

    public CrisisSeverity Severity { get; set; }

    public string Location { get; set; } = string.Empty;

    public int AffectedPopulation { get; set; }

    public CrisisStatus Status { get; set; } = CrisisStatus.Active;

    public DateTime StartDateUtc { get; set; }

    public DateTime? ResolvedDateUtc { get; set; }

    public float Latitude { get; set; }

    public float Longitude { get; set; }
}