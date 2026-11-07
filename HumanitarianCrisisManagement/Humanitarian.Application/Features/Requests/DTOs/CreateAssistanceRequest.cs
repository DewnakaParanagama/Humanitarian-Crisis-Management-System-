namespace Humanitarian.Application.Features.Requests.DTOs;

public class CreateAssistanceRequest
{
    public string Category { get; set; } = string.Empty;

    public string Urgency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int Affected { get; set; } = 1;
}