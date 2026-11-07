using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class AssistanceRequest : BaseEntity
{
    public Guid CitizenId { get; set; }

public Guid? CrisisId { get; set; }

    public RequestCategory Category { get; set; }

    public RequestPriority PriorityUrgency { get; set; } = RequestPriority.Medium;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int AffectedPeopleCount { get; set; } = 1;

    public RequestStatus Status { get; set; } = RequestStatus.Submitted;

    public Guid? AssignedOrganizationId { get; set; }

    public string? AIRecommendation { get; set; }

    public decimal? AIConfidenceScore { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public ApplicationUser? Citizen { get; set; }

    public Crisis? Crisis { get; set; }


}
