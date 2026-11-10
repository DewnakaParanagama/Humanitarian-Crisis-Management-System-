namespace Humanitarian.Application.Features.Crises.DTOs;

public class CreateCrisisDto
{
    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";

    public string? Description { get; set; }

    public int AffectedCount { get; set; }
}