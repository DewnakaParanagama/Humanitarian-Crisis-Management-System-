namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class DispatchVolunteerDto
{
    public Guid TargetId { get; set; }

    public string TargetType { get; set; } = string.Empty;
}