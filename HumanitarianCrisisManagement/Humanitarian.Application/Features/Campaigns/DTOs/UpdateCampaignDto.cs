namespace Humanitarian.Application.Features.Campaigns.DTOs;

public class UpdateCampaignDto
{
    public string? Name { get; set; }
    public decimal TargetAmount { get; set; }
    public string? Status { get; set; }
}