using System.Security.Claims;
using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Requests.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/requests")]
[Authorize(Policy = "CitizenOnly")]
public class RequestsController : ControllerBase
{
    private readonly IAssistanceRequestService _requestService;

    public RequestsController(
        IAssistanceRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest(
        [FromBody] CreateAssistanceRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var citizenId))
        {
            return Unauthorized();
        }

        var requestId = await _requestService.CreateRequestAsync(
            citizenId,
            request,
            cancellationToken);

        return StatusCode(201, new
        {
            id = requestId,
            message = "Assistance request submitted successfully.",
            status = "submitted"
        });
    }
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests(
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var citizenId))
        {
            return Unauthorized();
        }

        var requests = await _requestService.GetMyRequestsAsync(
            citizenId,
            cancellationToken);

        return Ok(requests);
    }
}