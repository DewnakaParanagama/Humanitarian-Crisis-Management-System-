namespace Humanitarian.Application.Features.Resources.DTOs;

public class CreateResourceDto
{
    public string ItemName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}