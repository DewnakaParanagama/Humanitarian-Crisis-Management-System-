namespace Humanitarian.Application.Features.Authentication.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

public AuthUserResponse User { get; set; } = new();


}

public class AuthUserResponse
{
    public Guid Id { get; set; }


public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;


}
