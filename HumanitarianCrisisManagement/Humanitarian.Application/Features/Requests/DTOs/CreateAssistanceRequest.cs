namespace Humanitarian.Application.Features.Requests.DTOs;

public class CreateAssistanceRequest
{
    public string Category { get; set; } = string.Empty;

    public string Urgency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int Affected { get; set; } = 1;

    public Guid? ShelterId { get; set; }
}