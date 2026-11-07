using Humanitarian.Domain.Common;

namespace Humanitarian.Application.Features.Users.DTOs;

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}