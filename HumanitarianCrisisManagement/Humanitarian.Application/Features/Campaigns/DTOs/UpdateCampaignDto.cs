namespace Humanitarian.Application.Features.Campaigns.DTOs;

public class UpdateCampaignDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal? CurrentAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? EndDate { get; set; }
}