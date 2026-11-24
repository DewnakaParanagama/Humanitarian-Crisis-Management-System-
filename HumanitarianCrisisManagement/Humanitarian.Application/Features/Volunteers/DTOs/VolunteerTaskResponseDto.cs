namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class VolunteerTaskResponseDto
{
    public Guid Id { get; set; }

    public string TargetType { get; set; } = string.Empty;

    public Guid TargetId { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; }
}