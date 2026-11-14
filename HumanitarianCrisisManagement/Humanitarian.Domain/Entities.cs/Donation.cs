using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class Donation : BaseEntity
{
    public string DonorName { get; set; } = string.Empty;

    public DonationType Type { get; set; }

    public decimal AmountOrValue { get; set; }

    public string? CampaignName { get; set; }

    // Foreign Key to Campaign (required for the relationship configuration)
    public Guid? CampaignId { get; set; }

    // Navigation property back to Campaign
    public Campaign? Campaign { get; set; }

    public DonationStatus Status { get; set; } = DonationStatus.Received;

    public DateTime DateReceivedUtc { get; set; } = DateTime.UtcNow;
}