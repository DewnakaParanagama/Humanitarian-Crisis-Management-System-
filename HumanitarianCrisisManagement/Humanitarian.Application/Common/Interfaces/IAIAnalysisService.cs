using Humanitarian.Application.Features.AI.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface IAIAnalysisService
{
    Task<AIAnalysisResponse> AnalyzeRequestAsync(
        AnalyzeRequestDto request,
        CancellationToken cancellationToken);
}