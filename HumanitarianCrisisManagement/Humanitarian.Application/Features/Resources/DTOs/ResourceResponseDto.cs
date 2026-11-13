namespace Humanitarian.Application.Features.Resources.DTOs;

public class ResourceResponseDto
{
    public Guid Id { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; }
}