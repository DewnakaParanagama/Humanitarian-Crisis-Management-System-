namespace Humanitarian.Application.Features.Donations.DTOs;

public class DonationResponseDto
{
    public Guid Id { get; set; } // Changed from int to Guid
    public string DonorName { get; set; } = "Anonymous";
    public string Type { get; set; } = string.Empty;
    public decimal AmountOrValue { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}