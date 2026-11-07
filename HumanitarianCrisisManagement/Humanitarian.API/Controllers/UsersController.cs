using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAllUsers(
        CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(
            cancellationToken);

        return Ok(users);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<UserResponse>> UpdateUserRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateUserRoleAsync(
            id,
            request,
            cancellationToken);

        return Ok(user);
    }
}