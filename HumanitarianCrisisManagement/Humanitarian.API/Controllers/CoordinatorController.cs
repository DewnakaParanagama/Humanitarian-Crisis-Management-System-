using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Coordinator.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/coordinator")]
[Authorize(Roles = "Coordinator,Admin")]
public class CoordinatorController : ControllerBase
{
    private readonly ICoordinatorService _coordinatorService;

    public CoordinatorController(
        ICoordinatorService coordinatorService)
    {
        _coordinatorService = coordinatorService;
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetAllRequests(
        CancellationToken cancellationToken)
    {
        var requests = await _coordinatorService.GetAllRequestsAsync(
            cancellationToken);

        return Ok(requests);
    }

    [HttpPut("requests/{id:guid}/status")]
    public async Task<IActionResult> UpdateRequestStatus(
        Guid id,
        [FromBody] UpdateRequestStatusDto request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Coordinator PUT Request ID: {id}");

        try
        {
            await _coordinatorService.UpdateRequestStatusAsync(
                id,
                request,
                cancellationToken);

            return Ok(new
            {
                message = "Request status updated successfully.",
                requestId = id,
                status = request.Status
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Assistance request not found."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats(
        CancellationToken cancellationToken)
    {
        var stats = await _coordinatorService.GetDashboardStatsAsync(
            cancellationToken);

        return Ok(stats);
    }
}