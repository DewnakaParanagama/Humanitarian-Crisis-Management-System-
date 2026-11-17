namespace Humanitarian.Application.Features.Requests.DTOs;

public class AssignedVolunteerDto
{
    public Guid VolunteerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = "Assigned";
    public string Skills { get; set; } = string.Empty;
}