namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class ActiveCitizenDto
{
    public Guid RequestId { get; set; }
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string ExactDistressLocation { get; set; } = string.Empty;
    public string EmergencyCategory { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}