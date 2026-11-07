using Humanitarian.Application.Features.Authentication.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
    RegisterRequest request,
    CancellationToken cancellationToken = default);


Task<AuthResponse> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default);


}
