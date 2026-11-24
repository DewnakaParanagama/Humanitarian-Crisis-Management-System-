using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class Resource : BaseEntity
{
    public string ItemName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = "Available";
}