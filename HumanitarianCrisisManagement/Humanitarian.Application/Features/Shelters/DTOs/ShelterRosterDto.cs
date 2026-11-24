namespace Humanitarian.Application.Features.Shelters.DTOs;

public class ShelterRosterDto
{
    public List<ShelterRosterMemberDto> Citizens { get; set; } = new();

    public List<ShelterRosterMemberDto> Volunteers { get; set; } = new();
}

public class ShelterRosterMemberDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}