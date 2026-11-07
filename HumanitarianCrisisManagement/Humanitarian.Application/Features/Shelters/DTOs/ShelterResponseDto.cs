namespace Humanitarian.Application.Features.Shelters.DTOs;

public class ShelterResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int Capacity { get; set; }

    public int Occupied { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<string> Facilities { get; set; } = new();

    public bool Accessible { get; set; }

    public string Contact { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;
}