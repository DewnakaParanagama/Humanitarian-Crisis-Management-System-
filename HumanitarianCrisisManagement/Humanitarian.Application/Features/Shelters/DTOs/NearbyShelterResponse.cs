namespace Humanitarian.Application.Features.Shelters.DTOs;

public class NearbyShelterResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int TotalCapacity { get; set; }

    public int CurrentOccupancy { get; set; }

    public int AvailableSpaces => TotalCapacity - CurrentOccupancy;

    public List<string> Facilities { get; set; } = new();

    public string Status { get; set; } = string.Empty;

    public double DistanceKm { get; set; }
}