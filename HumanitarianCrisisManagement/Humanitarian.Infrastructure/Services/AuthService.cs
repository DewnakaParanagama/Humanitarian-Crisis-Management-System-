using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Authentication.DTOs;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;


public AuthService(
    IAppDbContext context,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(
            user => user.Email == email,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var role = request.Role?.Trim();

        if (role != "Citizen" && role != "Volunteer")
        {
            throw new InvalidOperationException(
                "Public registration allows only Citizen or Volunteer roles.");
        }

        var user = new ApplicationUser
        {
            FullName = request.Name.Trim(),
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = role == "Volunteer"
                ? UserRole.Volunteer
                : UserRole.Citizen,
            Status = UserStatus.Active
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(
                item => item.Email == email,
                cancellationToken);

        if (user is null ||
            !_passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "This account is not active.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(ApplicationUser user)
    {
        return new AuthResponse
        {
            Token = _tokenService.GenerateToken(
                user.Id,
                user.Email,
                user.Role.ToString()),

            User = new AuthUserResponse
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }


}
