namespace Humanitarian.Application.Features.Shelters.DTOs;

public class CreateShelterDto
{
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Capacity { get; set; }

    public string Contact { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public bool Accessible { get; set; }

    public List<string> Facilities { get; set; } = new();
}