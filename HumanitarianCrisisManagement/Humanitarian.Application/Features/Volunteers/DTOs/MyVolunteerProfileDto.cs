namespace Humanitarian.Application.Features.Volunteers.DTOs;

public class MyVolunteerProfileDto
{
    public Guid Id { get; set; }

    public string VerificationStatus { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = new();

    public int CompletedTasks { get; set; }
}