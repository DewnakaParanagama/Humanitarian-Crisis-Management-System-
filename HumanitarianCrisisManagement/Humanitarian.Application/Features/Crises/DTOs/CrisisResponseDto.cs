namespace Humanitarian.Application.Features.Crises.DTOs;

public class CrisisResponseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int AffectedCount { get; set; }

    public int RequestsCount { get; set; }

    public float Latitude { get; set; }

    public float Longitude { get; set; }
}