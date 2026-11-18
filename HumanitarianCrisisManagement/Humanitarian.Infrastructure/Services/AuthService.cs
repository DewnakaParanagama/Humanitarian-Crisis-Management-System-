using System.Security.Cryptography;
using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Authentication.DTOs;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Humanitarian.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
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

        var isCitizen = role == "Citizen";
        string? otpCode = null;
        DateTime? otpExpiry = null;

        if (isCitizen)
        {
            otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            otpExpiry = DateTime.UtcNow.AddMinutes(10);
        }

        var user = new ApplicationUser
        {
            FullName = request.Name.Trim(),
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = isCitizen
                ? UserRole.Citizen
                : UserRole.Volunteer,
            Status = isCitizen
                ? UserStatus.Pending
                : UserStatus.Active,
            OtpCode = otpCode,
            OtpExpiryUtc = otpExpiry
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        if (isCitizen)
        {
            // Log OTP for testing/development until email/SMS provider is configured
            _logger.LogInformation(
                "OTP generated for Citizen {Email}: {OtpCode} (expires at {Expiry} UTC)",
                user.Email,
                otpCode,
                otpExpiry);

            return new AuthResponse
            {
                Token = string.Empty,
                User = new AuthUserResponse
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }

        return CreateAuthResponse(user);
    }

    public async Task<bool> VerifyOtpAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = code.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(
                item => item.Email == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (user.Status != UserStatus.Pending)
        {
            throw new InvalidOperationException(
                "Account is already verified or cannot be verified.");
        }

        if (string.IsNullOrEmpty(user.OtpCode) ||
            user.OtpCode != normalizedCode ||
            user.OtpExpiryUtc is null ||
            user.OtpExpiryUtc < DateTime.UtcNow)
        {
            return false;
        }

        user.Status = UserStatus.Active;
        user.OtpCode = null;
        user.OtpExpiryUtc = null;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
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

        if (user.Status == UserStatus.Pending)
        {
            throw new UnauthorizedAccessException(
                "Account is pending OTP verification. Please verify your account before logging in.");
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