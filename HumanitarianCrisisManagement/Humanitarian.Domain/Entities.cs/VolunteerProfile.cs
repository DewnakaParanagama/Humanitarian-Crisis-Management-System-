using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class VolunteerProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public List<string> Skills { get; set; } = new();

    public VolunteerAvailability Availability { get; set; }
        = VolunteerAvailability.Offline;

    public string AvailabilityDetails { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public List<string> Assets { get; set; } = new();

    public string? Experience { get; set; }
    public bool HasVehicle { get; set; }

    public string? VehicleType { get; set; }

    public VolunteerVerificationStatus VerificationStatus { get; set; }
        = VolunteerVerificationStatus.Pending;

    public int TasksCompleted { get; set; }

    public string? CurrentLocation { get; set; }

    public ApplicationUser? User { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LocationLastUpdatedUtc { get; set; }
}