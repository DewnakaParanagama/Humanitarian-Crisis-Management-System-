using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.AI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Authorize(Policy = "CitizenOnly")]
public class AIController : ControllerBase
{
    private readonly IAIAnalysisService _aiAnalysisService;

    public AIController(IAIAnalysisService aiAnalysisService)
    {
        _aiAnalysisService = aiAnalysisService;
    }

    [HttpPost("analyze-request")]
    public async Task<ActionResult<AIAnalysisResponse>> AnalyzeRequest(
        [FromBody] AnalyzeRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _aiAnalysisService.AnalyzeRequestAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
} 