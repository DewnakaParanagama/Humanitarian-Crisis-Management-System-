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

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int AffectedPeopleCount { get; set; } = 1;

    public RequestStatus Status { get; set; } = RequestStatus.Submitted;

    public Guid? AssignedOrganizationId { get; set; }

    public Guid? AssignedVolunteerId { get; set; }

    public Guid? AssignedShelterId { get; set; }

    public string? AssignedShelterName { get; set; }

    public string? AIRecommendation { get; set; }

    public decimal? AIConfidenceScore { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public ApplicationUser? Citizen { get; set; }

    public ApplicationUser? AssignedVolunteer { get; set; }

    public Shelter? AssignedShelter { get; set; }

    public Crisis? Crisis { get; set; }
}