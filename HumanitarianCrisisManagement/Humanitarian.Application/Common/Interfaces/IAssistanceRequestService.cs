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
}