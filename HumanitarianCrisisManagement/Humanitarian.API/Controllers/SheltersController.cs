using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Shelters.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/shelters")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _shelterService;

    public SheltersController(IShelterService shelterService)
    {
        _shelterService = shelterService;
    }
    [Authorize(Policy = "CitizenOnly")]
    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyList<NearbyShelterResponse>>> GetNearbyShelters(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusKm = 10,
        CancellationToken cancellationToken = default)
    {
        var shelters = await _shelterService.GetNearbySheltersAsync(
            latitude,
            longitude,
            radiusKm,
            cancellationToken);

        return Ok(shelters);
    }
    [Authorize(Roles = "Coordinator,Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShelterResponseDto>>> GetAllShelters(
    CancellationToken cancellationToken)
    {
        var shelters = await _shelterService.GetAllSheltersAsync(
            cancellationToken);

        return Ok(shelters);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ShelterResponseDto>> CreateShelter(
    [FromBody] CreateShelterDto request,
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var adminId))
            return Unauthorized("Invalid user ID in token.");

        var shelter = await _shelterService.CreateShelterAsync(
            request,
            adminId,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAllShelters),
            shelter);
    }
    [Authorize(Roles = "Coordinator,Admin")]
    [HttpPut("{id:guid}/allocate")]
    public async Task<ActionResult<ShelterResponseDto>> AllocatePeople(
    Guid id,
    [FromBody] AllocateShelterDto request,
    CancellationToken cancellationToken)
    {
        var shelter = await _shelterService.AllocatePeopleAsync(
            id,
            request,
            cancellationToken);

        return Ok(shelter);
    }
}