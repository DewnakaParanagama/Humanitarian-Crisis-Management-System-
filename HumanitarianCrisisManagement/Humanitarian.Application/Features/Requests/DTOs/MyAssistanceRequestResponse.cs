namespace Humanitarian.Application.Features.Requests.DTOs;

public class MyAssistanceRequestResponse
{
    public Guid Id { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? AssignedShelterName { get; set; }

    public int AffectedPeopleCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}