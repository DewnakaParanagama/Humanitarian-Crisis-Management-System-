using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class Shelter : BaseEntity
{
    public string Name { get; set; } = string.Empty;
public string Location { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int TotalCapacity { get; set; }

    public int CurrentOccupancy { get; set; }

    public List<string> Facilities { get; set; } = new();

    public ShelterStatus Status { get; set; } = ShelterStatus.Open;

    public Guid ShelterAdminId { get; set; }

    public ApplicationUser? ShelterAdmin { get; set; }

    public string District { get; set; } = string.Empty;

    public string Contact { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public bool Accessible { get; set; }

}
