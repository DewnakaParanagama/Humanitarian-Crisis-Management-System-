using Humanitarian.Application.Features.Coordinator.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface ICoordinatorService
{
    Task<IReadOnlyList<CoordinatorRequestResponse>> GetAllRequestsAsync(
        CancellationToken cancellationToken);

    Task UpdateRequestStatusAsync(
        Guid requestId,
        UpdateRequestStatusDto request,
        CancellationToken cancellationToken);

    Task<CoordinatorDashboardStatsDto> GetDashboardStatsAsync(
        CancellationToken cancellationToken);
}