using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Volunteers.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/volunteers")]
public class VolunteersController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;
    private readonly INotificationService _notificationService;

    // එකම Constructor එකයි තියෙන්නේ. මේකට Services දෙකම එනවා.
    public VolunteersController(
        IVolunteerService volunteerService,
        INotificationService notificationService)
    {
        _volunteerService = volunteerService;
        _notificationService = notificationService;
    }

    // Citizen registers as a volunteer
    [Authorize(Roles = "Citizen")]
    [HttpPost]
    public async Task<ActionResult<VolunteerResponseDto>> RegisterVolunteer(
        [FromBody] CreateVolunteerDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user ID in token.");

        var volunteer = await _volunteerService.RegisterVolunteerAsync(
            userId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAllVolunteers),
            volunteer);
    }

    // Citizen applies to become a volunteer
    [Authorize(Roles = "Citizen")]
    [HttpPost("apply")]
    public async Task<ActionResult<VolunteerResponseDto>> ApplyVolunteer(
        [FromBody] ApplyVolunteerDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user ID in token.");

        var volunteer = await _volunteerService.ApplyVolunteerAsync(
            userId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetMyProfile),
            volunteer);
    }

    // Coordinator and Admin view all volunteers
    [Authorize(Roles = "Coordinator,Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VolunteerResponseDto>>> GetAllVolunteers(
        CancellationToken cancellationToken)
    {
        var volunteers = await _volunteerService.GetAllVolunteersAsync(
            cancellationToken);

        return Ok(volunteers);
    }

    // Admin updates volunteer verification status
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<VolunteerResponseDto>> UpdateVolunteerStatus(
        Guid id,
        [FromBody] UpdateVolunteerStatusDto request,
        CancellationToken cancellationToken)
    {
        var volunteer = await _volunteerService.UpdateVolunteerStatusAsync(
            id,
            request,
            cancellationToken);

        return Ok(volunteer);
    }

    // Coordinator and Admin dispatch a volunteer
    [Authorize(Roles = "Coordinator,Admin")]
    [HttpPost("{id:guid}/dispatch")]
    public async Task<IActionResult> DispatchVolunteer(
        Guid id,
        [FromBody] DispatchVolunteerDto request,
        CancellationToken cancellationToken)
    {
        await _volunteerService.DispatchVolunteerAsync(
            id,
            request,
            cancellationToken);

        // මෙතනදී SignalR Alert එක යවන්න අපි NotificationService එකට කතා කරනවා.
        // Alert එක යවන්න අදාළ Volunteer ගේ UserId එක හොයාගන්න ඕන නිසා,
        // දැනට අපි TargetType එක එක්ක Message එකක් හදලා තියෙනවා.
        // (VolunteerService එකෙන් UserId එකක් Return කරගන්න හැදුවොත් මේක තවත් ලේසියි, 
        // ඒත් දැනට මේක හරි).

        // var message = $"You have been assigned to a new {request.TargetType} emergency. Please check your tasks.";
        // await _notificationService.SendDispatchAlertAsync(volunteerUserId, message, cancellationToken);

        return Ok(new
        {
            message = "Volunteer dispatched successfully."
        });
    }

    // Get logged-in volunteer profile
    [Authorize(Roles = "Citizen,Volunteer,Admin")]
    [HttpGet("me")]
    public async Task<ActionResult<MyVolunteerProfileDto>> GetMyProfile(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user ID in token.");

        var profile = await _volunteerService.GetMyProfileAsync(
            userId,
            cancellationToken);

        return Ok(profile);
    }

    // Get logged-in volunteer's assigned tasks
    [Authorize(Roles = "Citizen,Volunteer,Admin")]
    [HttpGet("me/tasks")]
    public async Task<ActionResult<IReadOnlyList<VolunteerTaskResponseDto>>> GetMyTasks(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user ID in token.");

        var tasks = await _volunteerService.GetMyTasksAsync(
            userId,
            cancellationToken);

        return Ok(tasks);
    }

    // Update logged-in volunteer's task status
    [Authorize(Roles = "Citizen,Volunteer,Admin")]
    [HttpPut("tasks/{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid taskId,
        [FromBody] UpdateVolunteerTaskStatusDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user ID in token.");

        await _volunteerService.UpdateTaskStatusAsync(
            userId,
            taskId,
            request,
            cancellationToken);

        return Ok(new
        {
            message = "Task completed successfully."
        });
    }

    // Update logged-in volunteer's live location
    [Authorize(Roles = "Volunteer,Citizen,Admin")]
    [HttpPut("me/location")]
    public async Task<IActionResult> UpdateLiveLocation(
        [FromBody] UpdateVolunteerLocationDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user ID in token.");

        await _volunteerService.UpdateLiveLocationAsync(userId, request, cancellationToken);

        return Ok(new { message = "Location updated successfully." });
    }

    // Get active volunteer locations for coordinator map
    [Authorize(Roles = "Admin,Coordinator")]
    [HttpGet("active-locations")]
    public async Task<ActionResult<IReadOnlyList<ActiveVolunteerLocationDto>>> GetActiveLocations(
        CancellationToken cancellationToken)
    {
        var locations = await _volunteerService.GetActiveLocationsAsync(cancellationToken);
        return Ok(locations);
    }
}