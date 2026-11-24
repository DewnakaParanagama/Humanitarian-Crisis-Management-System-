using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }


public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string? PreviousValue { get; set; }

    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }


}
