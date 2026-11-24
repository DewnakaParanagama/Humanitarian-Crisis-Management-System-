using Humanitarian.Application.Features.Resources.DTOs;

namespace Humanitarian.Application.Interfaces;

public interface IResourceService
{
    Task<IReadOnlyList<ResourceResponseDto>> GetAllResourcesAsync(
        CancellationToken cancellationToken);

    Task<ResourceResponseDto> CreateResourceAsync(
        CreateResourceDto request,
        CancellationToken cancellationToken);

    Task<ResourceResponseDto> DispatchResourceAsync(
        Guid id,
        DispatchResourceDto request,
        CancellationToken cancellationToken);
}