namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class ActiveVolunteerLocationDto
{
    public Guid VolunteerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}