using Humanitarian.Application.Features.Shelters.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface IShelterService
{
    // Get nearby shelters
    Task<IReadOnlyList<NearbyShelterResponse>> GetNearbySheltersAsync(
        double latitude,
        double longitude,
        double radiusKm,
        CancellationToken cancellationToken);

    // Get all shelters
    Task<IReadOnlyList<ShelterResponseDto>> GetAllSheltersAsync(
        CancellationToken cancellationToken);

    // Create a new shelter
    Task<ShelterResponseDto> CreateShelterAsync(
     CreateShelterDto request,
     Guid shelterAdminId,
     CancellationToken cancellationToken);

    // Allocate people to a shelter
    Task<ShelterResponseDto> AllocatePeopleAsync(
        Guid shelterId,
        AllocateShelterDto request,
        CancellationToken cancellationToken);
}