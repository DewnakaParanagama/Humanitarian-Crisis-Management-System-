using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Requests.DTOs;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class AssistanceRequestService : IAssistanceRequestService
{
    private readonly IAppDbContext _context;

    public AssistanceRequestService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateRequestAsync(
        Guid citizenId,
        CreateAssistanceRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<RequestCategory>(
                request.Category, true, out var category) ||
            !Enum.IsDefined(typeof(RequestCategory), category))
        {
            throw new ArgumentException("Invalid request category.");
        }

        if (!Enum.TryParse<RequestPriority>(
                request.Urgency, true, out var urgency) ||
            !Enum.IsDefined(typeof(RequestPriority), urgency))
        {
            throw new ArgumentException("Invalid request urgency.");
        }

        if (string.IsNullOrWhiteSpace(request.Description) ||
            request.Description.Length > 2000)
        {
            throw new ArgumentException("Invalid description.");
        }

        if (string.IsNullOrWhiteSpace(request.Location) ||
            request.Location.Length > 300)
        {
            throw new ArgumentException("Invalid location.");
        }

        if (request.Affected < 1)
        {
            throw new ArgumentException(
                "Affected count must be at least 1.");
        }

        var citizenExists = await _context.Users
            .AnyAsync(user => user.Id == citizenId &&
                              user.Role == UserRole.Citizen &&
                              user.Status == UserStatus.Active,
                cancellationToken);

        if (!citizenExists)
        {
            throw new UnauthorizedAccessException(
                "Active citizen account not found.");
        }

        var assistanceRequest = new AssistanceRequest
        {
            CitizenId = citizenId,
            Category = category,
            PriorityUrgency = urgency,
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            AffectedPeopleCount = request.Affected,
            Status = RequestStatus.Submitted
        };

        _context.AssistanceRequests.Add(assistanceRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return assistanceRequest.Id;
    }
    public async Task<IReadOnlyList<MyAssistanceRequestResponse>> GetMyRequestsAsync(
    Guid citizenId,
    CancellationToken cancellationToken)
    {
        var requests = await _context.AssistanceRequests
            .AsNoTracking()
            .Where(request => request.CitizenId == citizenId)
            .OrderByDescending(request => request.CreatedAtUtc)
            .Select(request => new MyAssistanceRequestResponse
            {
                Id = request.Id,
                Category = request.Category.ToString(),
                Priority = request.PriorityUrgency.ToString(),
                Description = request.Description,
                Location = request.Location,
                AffectedPeopleCount = request.AffectedPeopleCount,
                Status = request.Status.ToString(),
                CreatedAtUtc = request.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return requests;
    }
}