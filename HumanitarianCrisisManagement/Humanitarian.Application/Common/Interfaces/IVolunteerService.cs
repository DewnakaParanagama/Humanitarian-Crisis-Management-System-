using Humanitarian.Application.Features.Volunteers.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface IVolunteerService
{
    // Register volunteer
    Task<VolunteerResponseDto> RegisterVolunteerAsync(
        Guid userId,
        CreateVolunteerDto request,
        CancellationToken cancellationToken);

    // Apply to become a volunteer
    Task<VolunteerResponseDto> ApplyVolunteerAsync(
        Guid userId,
        ApplyVolunteerDto request,
        CancellationToken cancellationToken);

    // Get all volunteers
    Task<IReadOnlyList<VolunteerResponseDto>> GetAllVolunteersAsync(
        CancellationToken cancellationToken);

    // Update volunteer verification status
    Task<VolunteerResponseDto> UpdateVolunteerStatusAsync(
        Guid volunteerId,
        UpdateVolunteerStatusDto request,
        CancellationToken cancellationToken);

    // Dispatch volunteer
    Task DispatchVolunteerAsync(
        Guid volunteerId,
        DispatchVolunteerDto request,
        CancellationToken cancellationToken);

    // Get logged-in volunteer profile
    Task<MyVolunteerProfileDto> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken);

    // Get logged-in volunteer tasks
    Task<IReadOnlyList<VolunteerTaskResponseDto>> GetMyTasksAsync(
        Guid userId,
        CancellationToken cancellationToken);

    // Update assigned task status
    Task UpdateTaskStatusAsync(
        Guid userId,
        Guid taskId,
        UpdateVolunteerTaskStatusDto request,
        CancellationToken cancellationToken);

    // Update live location
    Task UpdateLiveLocationAsync(
        Guid userId,
        UpdateVolunteerLocationDto request,
        CancellationToken cancellationToken);

    // Get all active volunteer locations for the map
    Task<IReadOnlyList<ActiveVolunteerLocationDto>> GetActiveLocationsAsync(
        CancellationToken cancellationToken);

    // Get citizens currently assigned to the logged-in volunteer
    Task<IReadOnlyList<ActiveCitizenDto>> GetActiveAssignedCitizensAsync(
        Guid userId,
        CancellationToken cancellationToken);
}