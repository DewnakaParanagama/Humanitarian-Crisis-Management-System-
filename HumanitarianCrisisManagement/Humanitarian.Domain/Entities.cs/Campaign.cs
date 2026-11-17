using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal RaisedAmount { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}