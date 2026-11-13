namespace Humanitarian.Application.Features.Campaigns.DTOs;

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public string? Status { get; set; }
}