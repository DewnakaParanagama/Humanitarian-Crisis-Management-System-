namespace Humanitarian.Application.Features.Authentication.DTOs;

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}