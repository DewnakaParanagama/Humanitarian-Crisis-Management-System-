namespace Humanitarian.Application.Features.Coordinator.DTOs;

public class CoordinatorRequestResponse
{
    public Guid Id { get; set; }

    public string CitizenName { get; set; } = string.Empty;

    public string CitizenPhone { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Urgency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int AffectedPeopleCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}