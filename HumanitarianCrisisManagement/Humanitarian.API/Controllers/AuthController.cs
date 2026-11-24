using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Authentication.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;


public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(
            request,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        var success = await _authService.VerifyOtpAsync(
            request.Email,
            request.Code,
            cancellationToken);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired OTP code, or user not found." });
        }

        return Ok(new { message = "Account verified successfully. You can now log in." });
    }

}
