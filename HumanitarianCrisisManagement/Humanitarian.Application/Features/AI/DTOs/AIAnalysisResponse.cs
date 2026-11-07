namespace Humanitarian.Application.Features.AI.DTOs;

public class AIAnalysisResponse
{
    public int ConfidenceScore { get; set; }

    public string CalculatedUrgency { get; set; } = string.Empty;

    public double UrgencyScore { get; set; }

    public string RecommendedAction { get; set; } = string.Empty;

    public string NearestResource { get; set; } = string.Empty;
}