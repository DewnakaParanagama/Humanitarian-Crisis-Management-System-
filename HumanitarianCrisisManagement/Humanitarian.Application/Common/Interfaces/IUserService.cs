using Humanitarian.Application.Features.Users.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(
        CancellationToken cancellationToken);

    Task<UserResponse> UpdateUserRoleAsync(
        Guid userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken);
}