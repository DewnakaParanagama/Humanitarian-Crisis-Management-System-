namespace Humanitarian.Application.Features.Campaigns.DTOs;

public class CampaignResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
}