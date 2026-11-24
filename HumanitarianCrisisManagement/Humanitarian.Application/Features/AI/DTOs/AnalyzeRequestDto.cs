namespace Humanitarian.Application.Features.AI.DTOs;

public class AnalyzeRequestDto
{
    public string Category { get; set; } = string.Empty;

    public string Urgency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int Affected { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string Language { get; set; } = "English";
}