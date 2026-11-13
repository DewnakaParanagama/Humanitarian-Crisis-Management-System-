namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class ApplyVolunteerDto
{
    public string District { get; set; } = string.Empty;

    public string Availability { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = new();

    public List<string> Assets { get; set; } = new();

    public string? Experience { get; set; }
}