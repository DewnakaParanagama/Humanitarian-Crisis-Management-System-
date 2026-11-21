namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class ActiveCitizenDto
{
    public Guid RequestId { get; set; }
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ExactDistressLocation { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string EmergencyCategory { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}