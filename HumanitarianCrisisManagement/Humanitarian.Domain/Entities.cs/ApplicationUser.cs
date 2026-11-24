using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Citizen;

    public UserStatus Status { get; set; } = UserStatus.Pending; // Make sure status defaults to Pending for new registrations

    public DateTime? LastLoginAtUtc { get; set; }   

    public string? OtpCode { get; set; }
    public DateTime? OtpExpiryUtc { get; set; }
     

}
