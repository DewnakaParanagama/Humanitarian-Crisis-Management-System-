using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Crises.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/crises")]
[Authorize(Roles = "Admin,Coordinator")]
public class CrisesController : ControllerBase
{
    private readonly ICrisisService _crisisService;

    public CrisesController(ICrisisService crisisService)
    {
        _crisisService = crisisService;
    }

    // GET: api/v1/crises
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CrisisResponseDto>>> GetAllCrises(
        CancellationToken cancellationToken)
    {
        var crises = await _crisisService.GetAllCrisesAsync(
            cancellationToken);

        return Ok(crises);
    }

    // POST: api/v1/crises
    [HttpPost]
    public async Task<ActionResult<CrisisResponseDto>> CreateCrisis(
        [FromBody] CreateCrisisDto request,
        CancellationToken cancellationToken)
    {
        var crisis = await _crisisService.CreateCrisisAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAllCrises),
            new { id = crisis.Id },
            crisis);
    }
}