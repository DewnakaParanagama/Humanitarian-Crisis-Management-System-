namespace Humanitarian.Application.Features.Coordinator.DTOs;

public class CoordinatorDashboardStatsDto
{
    public int TotalActiveCrises { get; set; }

    public int PendingRequests { get; set; }

    public int ActiveVolunteers { get; set; }

    public int AvailableResources { get; set; }
}