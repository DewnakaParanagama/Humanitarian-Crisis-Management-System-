using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}