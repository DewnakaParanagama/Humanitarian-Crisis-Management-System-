using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class VolunteerAssignment : BaseEntity
{
    public Guid VolunteerProfileId { get; set; }

    public Guid TargetId { get; set; }

    public string TargetType { get; set; } = string.Empty;

    public string Status { get; set; } = "Assigned";

    public DateTime DispatchedAtUtc { get; set; }
        = DateTime.UtcNow;

    public VolunteerProfile? VolunteerProfile { get; set; }
}