using Humanitarian.Application.Features.Requests.DTOs;

namespace Humanitarian.Application.Common.Interfaces;

public interface IAssistanceRequestService
{
    Task<Guid> CreateRequestAsync(
        Guid citizenId,
        CreateAssistanceRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MyAssistanceRequestResponse>> GetMyRequestsAsync(
        Guid citizenId,
        CancellationToken cancellationToken);

    Task<AssignedVolunteerDto?> GetAssignedVolunteerForRequestAsync(
        Guid citizenId,
        Guid requestId,
        CancellationToken cancellationToken);

    // Shelter Requests
    Task<Guid> CreateShelterRequestAsync(
        Guid citizenId,
        Guid shelterId,
        CreateAssistanceRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MyAssistanceRequestResponse>> GetShelterRequestsAsync(
        Guid shelterId,
        Guid shelterAdminId,
        CancellationToken cancellationToken);

    Task<bool> VerifyShelterRequestAsync(
        Guid shelterId,
        Guid requestId,
        Guid shelterAdminId,
        CancellationToken cancellationToken);
}