using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Coordinator.DTOs;
using Humanitarian.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class CoordinatorService : ICoordinatorService
{
    private readonly IAppDbContext _context;

    public CoordinatorService(IAppDbContext context)
    {
        _context = context;
    }

    // Get all assistance requests
    public async Task<IReadOnlyList<CoordinatorRequestResponse>>
        GetAllRequestsAsync(CancellationToken cancellationToken)
    {
        var requests = await _context.AssistanceRequests
            .AsNoTracking()
            .OrderBy(request => request.PriorityUrgency)
            .ThenByDescending(request => request.CreatedAtUtc)
            .Select(request => new CoordinatorRequestResponse
            {
                Id = request.Id,

                CitizenName = request.Citizen != null
                    ? request.Citizen.FullName
                    : "Unknown",

                CitizenPhone = request.Citizen != null
                    ? request.Citizen.PhoneNumber
                    : "Not available",

                Category = request.Category.ToString(),

                Urgency = request.PriorityUrgency.ToString(),

                Description = request.Description,

                Location = request.Location,

                Latitude = request.Latitude,

                Longitude = request.Longitude,

                AssignedShelterName = request.AssignedShelterName ?? (request.AssignedShelter != null ? request.AssignedShelter.Name : null),

                AffectedPeopleCount = request.AffectedPeopleCount,

                Status = request.Status == RequestStatus.Review
                    ? "Under Review"
                    : request.Status.ToString(),

                CreatedAtUtc = request.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return requests;
    }

    // Update assistance request status
    public async Task UpdateRequestStatusAsync(
        Guid requestId,
        UpdateRequestStatusDto request,
        CancellationToken cancellationToken)
    {
        var assistanceRequest = await _context.AssistanceRequests
            .FirstOrDefaultAsync(
                item => item.Id == requestId,
                cancellationToken);

        if (assistanceRequest == null)
        {
            throw new KeyNotFoundException(
                "Assistance request not found.");
        }

        var inputStatus = request.Status?.Trim()
            .Replace(" ", "")
            .Replace("_", "");

        if (string.Equals(
                inputStatus,
                "UnderReview",
                StringComparison.OrdinalIgnoreCase))
        {
            inputStatus = "Review";
        }

        if (!Enum.TryParse<RequestStatus>(
                inputStatus,
                true,
                out var newStatus) ||
            !Enum.IsDefined(typeof(RequestStatus), newStatus))
        {
            throw new ArgumentException("Invalid request status.");
        }

        // Validate status transition
        var validTransition = assistanceRequest.Status switch
        {
            RequestStatus.Submitted =>
    newStatus == RequestStatus.Review ||
    newStatus == RequestStatus.Assigned,

            RequestStatus.Review =>
                newStatus == RequestStatus.Verified,

            RequestStatus.Verified =>
                newStatus == RequestStatus.Assigned,

            RequestStatus.Assigned =>
                newStatus == RequestStatus.InProgress,

            RequestStatus.InProgress =>
                newStatus == RequestStatus.Delivered,

            RequestStatus.Delivered =>
                newStatus == RequestStatus.Closed,

            _ => false
        };

        if (!validTransition)
        {
            throw new InvalidOperationException(
                $"Cannot change status from {assistanceRequest.Status} to {newStatus}.");
        }

        // Assign volunteer to the assistance request
        if (newStatus == RequestStatus.Assigned)
        {
            if (request.VolunteerId == null)
            {
                throw new ArgumentException(
                    "Volunteer ID is required when assigning a request.");
            }

            var volunteer = await _context.VolunteerProfiles
                .FirstOrDefaultAsync(
                    v => v.Id == request.VolunteerId.Value,
                    cancellationToken);

            if (volunteer == null)
            {
                throw new KeyNotFoundException(
                    "Volunteer not found.");
            }

            // Store Volunteer User ID in AssignedVolunteerId
            assistanceRequest.AssignedVolunteerId = volunteer.UserId;
        }

        // Update request status
        assistanceRequest.Status = newStatus;

        if (newStatus == RequestStatus.Delivered)
        {
            assistanceRequest.DeliveredAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Get dashboard statistics
    public async Task<CoordinatorDashboardStatsDto>
        GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var activeCrises = await _context.Crises
            .CountAsync(
                crisis => crisis.Status == CrisisStatus.Active,
                cancellationToken);

        var pendingRequests = await _context.AssistanceRequests
            .CountAsync(
                request =>
                    request.Status == RequestStatus.Submitted ||
                    request.Status == RequestStatus.Review ||
                    request.Status == RequestStatus.Verified ||
                    request.Status == RequestStatus.Assigned,
                cancellationToken);

        var activeVolunteers = await _context.VolunteerProfiles
            .CountAsync(
                volunteer =>
                    volunteer.Availability == VolunteerAvailability.Available &&
                    (volunteer.VerificationStatus == VolunteerVerificationStatus.Verified ||
                     volunteer.VerificationStatus == VolunteerVerificationStatus.Active),
                cancellationToken);

        var availableResources = await _context.Shelters
            .Where(shelter => shelter.Status == ShelterStatus.Open)
            .SumAsync(
                shelter => shelter.TotalCapacity - shelter.CurrentOccupancy,
                cancellationToken);

        return new CoordinatorDashboardStatsDto
        {
            TotalActiveCrises = activeCrises,
            PendingRequests = pendingRequests,
            ActiveVolunteers = activeVolunteers,
            AvailableResources = availableResources
        };
    }
}