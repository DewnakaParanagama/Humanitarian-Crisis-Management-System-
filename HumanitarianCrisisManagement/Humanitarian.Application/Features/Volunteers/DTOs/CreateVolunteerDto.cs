namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class CreateVolunteerDto
{
    public List<string> Skills { get; set; } = new();

    public string Availability { get; set; } = string.Empty;

    public bool HasVehicle { get; set; }

    public string? VehicleType { get; set; }

    public string Location { get; set; } = string.Empty;
}