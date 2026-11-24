namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class VolunteerResponseDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = new();

    public string Availability { get; set; } = string.Empty;

    public bool HasVehicle { get; set; }

    public string? VehicleType { get; set; }

    public string Location { get; set; } = string.Empty;

    public string VerificationStatus { get; set; } = string.Empty;

    public int TasksCompleted { get; set; }
}