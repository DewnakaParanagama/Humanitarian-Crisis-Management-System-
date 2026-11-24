using Humanitarian.Application.Features.Crises.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface ICrisisService
{
    Task<IReadOnlyList<CrisisResponseDto>> GetAllCrisesAsync(
        CancellationToken cancellationToken);

    Task<CrisisResponseDto> CreateCrisisAsync(
        CreateCrisisDto request,
        CancellationToken cancellationToken);
}