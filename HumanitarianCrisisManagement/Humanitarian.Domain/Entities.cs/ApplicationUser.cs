using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Citizen;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAtUtc { get; set; }   

}
