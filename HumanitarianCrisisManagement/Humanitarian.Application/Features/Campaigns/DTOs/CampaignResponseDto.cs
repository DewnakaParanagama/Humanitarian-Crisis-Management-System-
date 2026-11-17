namespace Humanitarian.Application.Features.Campaigns.DTOs;

public class CampaignResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal RaisedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}