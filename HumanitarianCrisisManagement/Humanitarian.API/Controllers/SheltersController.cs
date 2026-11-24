using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Requests.DTOs;
using Humanitarian.Application.Features.Shelters.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/shelters")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _shelterService;
    private readonly IAssistanceRequestService _assistanceRequestService;

    public SheltersController(
        IShelterService shelterService,
        IAssistanceRequestService assistanceRequestService)
    {
        _shelterService = shelterService;
        _assistanceRequestService = assistanceRequestService;
    }

    // =====================================================
    // GET NEARBY SHELTERS
    // =====================================================
    [Authorize(Policy = "CitizenOnly")]
    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyList<NearbyShelterResponse>>>
        GetNearbyShelters(
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

    // =====================================================
    // GET ALL SHELTERS
    // =====================================================
    [Authorize(Roles = "Coordinator,Admin,ShelterAdmin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShelterResponseDto>>>
        GetAllShelters(
            CancellationToken cancellationToken)
    {
        var shelters = await _shelterService.GetAllSheltersAsync(
            cancellationToken);

        return Ok(shelters);
    }

    // =====================================================
    // CREATE SHELTER
    // =====================================================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ShelterResponseDto>> CreateShelter(
        [FromBody] CreateShelterDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var adminId))
        {
            return Unauthorized("Invalid user ID in token.");
        }

        var shelter = await _shelterService.CreateShelterAsync(
            request,
            adminId,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAllShelters),
            shelter);
    }

    // =====================================================
    // ALLOCATE PEOPLE
    // =====================================================
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

    // =====================================================
    // CREATE DIRECT SHELTER REQUEST
    // Citizen sends a request to a specific shelter
    // =====================================================
    [Authorize(Policy = "CitizenOnly")]
    [HttpPost("{id:guid}/requests")]
    public async Task<IActionResult> CreateShelterRequest(
        Guid id,
        [FromBody] CreateAssistanceRequest request,
        CancellationToken cancellationToken)
    {
        var citizenIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(citizenIdValue, out var citizenId))
        {
            return Unauthorized("Invalid citizen ID in token.");
        }

        var requestId =
            await _assistanceRequestService.CreateShelterRequestAsync(
                citizenId,
                id,
                request,
                cancellationToken);

        return Ok(new
        {
            RequestId = requestId,
            Message = "Shelter request submitted successfully."
        });
    }

    // =====================================================
    // GET SHELTER REQUESTS
    // Shelter manager views requests for their shelter
    // =====================================================
    [Authorize(Roles = "ShelterAdmin")]
    [HttpGet("{id:guid}/requests")]
    public async Task<ActionResult<IReadOnlyList<MyAssistanceRequestResponse>>>
        GetShelterRequests(
            Guid id,
            CancellationToken cancellationToken)
    {
        var shelterAdminIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(shelterAdminIdValue, out var shelterAdminId))
        {
            return Unauthorized("Invalid Shelter Admin ID in token.");
        }

        var requests =
            await _assistanceRequestService.GetShelterRequestsAsync(
                id,
                shelterAdminId,
                cancellationToken);

        return Ok(requests);
    }

    // =====================================================
    // VERIFY SHELTER REQUEST
    // Shelter manager verifies a submitted request
    // =====================================================
    [Authorize(Roles = "ShelterAdmin")]
    [HttpPut("{id:guid}/requests/{requestId:guid}/verify")]
    public async Task<IActionResult> VerifyShelterRequest(
        Guid id,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var shelterAdminIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(shelterAdminIdValue, out var shelterAdminId))
        {
            return Unauthorized("Invalid Shelter Admin ID in token.");
        }

        var verified =
            await _assistanceRequestService.VerifyShelterRequestAsync(
                id,
                requestId,
                shelterAdminId,
                cancellationToken);

        if (!verified)
        {
            return BadRequest("Shelter request verification failed.");
        }

        return Ok(new
        {
            RequestId = requestId,
            Message = "Shelter request verified successfully."
        });
    }

    // =====================================================
    // GET SHELTER ROSTER
    // Shelter manager views citizens and volunteers
    // =====================================================
    [Authorize(Roles = "ShelterAdmin")]
    [HttpGet("{id:guid}/roster")]
    public async Task<ActionResult<ShelterRosterDto>> GetShelterRoster(
        Guid id,
        CancellationToken cancellationToken)
    {
        var shelterAdminIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(shelterAdminIdValue, out var shelterAdminId))
        {
            return Unauthorized("Invalid Shelter Admin ID in token.");
        }

        var roster = await _shelterService.GetShelterRosterAsync(
            id,
            shelterAdminId,
            cancellationToken);

        return Ok(roster);
    }
}