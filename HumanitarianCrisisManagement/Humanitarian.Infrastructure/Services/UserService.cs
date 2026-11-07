using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Users.DTOs;
using Humanitarian.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;

    public UserService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .Select(user => new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserResponse> UpdateUserRoleAsync(
        Guid userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(UserRole), request.Role))
        {
            throw new ArgumentException("Invalid user role.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.Role = request.Role;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Status = user.Status.ToString()
        };
    }
}