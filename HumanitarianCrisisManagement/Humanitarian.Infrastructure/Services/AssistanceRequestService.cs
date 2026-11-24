using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.AI.DTOs;
using Humanitarian.Application.Features.Requests.DTOs;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class AssistanceRequestService : IAssistanceRequestService
{
    private readonly IAppDbContext _context;
    private readonly IAIAnalysisService _aiAnalysisService;

    public AssistanceRequestService(
        IAppDbContext context,
        IAIAnalysisService aiAnalysisService)
    {
        _context = context;
        _aiAnalysisService = aiAnalysisService;
    }

    // =========================================================
    // CREATE NORMAL ASSISTANCE REQUEST
    // =========================================================
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

        if (request.Latitude.HasValue &&
            (request.Latitude < -90 || request.Latitude > 90))
        {
            throw new ArgumentException("Invalid latitude.");
        }

        if (request.Longitude.HasValue &&
            (request.Longitude < -180 || request.Longitude > 180))
        {
            throw new ArgumentException("Invalid longitude.");
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

        // Validate selected shelter if provided
        Shelter? selectedShelter = null;

        if (request.ShelterId.HasValue)
        {
            selectedShelter = await _context.Shelters
                .FirstOrDefaultAsync(
                    shelter => shelter.Id == request.ShelterId.Value,
                    cancellationToken);

            if (selectedShelter == null)
            {
                throw new ArgumentException(
                    "Selected shelter was not found.");
            }

            if (category != RequestCategory.Shelter)
            {
                throw new ArgumentException(
                    "A shelter can only be selected for shelter requests.");
            }
        }

        var assistanceRequest = new AssistanceRequest
        {
            CitizenId = citizenId,
            Category = category,
            PriorityUrgency = urgency,
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AffectedPeopleCount = request.Affected,
            Status = RequestStatus.Submitted,

            AssignedShelterId = selectedShelter?.Id,
            AssignedShelterName = selectedShelter?.Name
        };

        var analysis = await _aiAnalysisService.AnalyzeRequestAsync(
            new AnalyzeRequestDto
            {
                Category = request.Category,
                Urgency = request.Urgency,
                Description = request.Description,
                Location = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Affected = request.Affected,
                Language = request.Language
            },
            cancellationToken);

        assistanceRequest.AISummary = analysis.Summary;
        assistanceRequest.AIRecommendation = analysis.RecommendedAction;
        assistanceRequest.AIConfidenceScore = analysis.ConfidenceScore;

        _context.AssistanceRequests.Add(assistanceRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return assistanceRequest.Id;
    }


    // =========================================================
    // GET CITIZEN'S OWN REQUESTS
    // =========================================================
    public async Task<IReadOnlyList<MyAssistanceRequestResponse>>
        GetMyRequestsAsync(
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
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AssignedShelterName = request.AssignedShelterName ??
                    (request.AssignedShelter != null
                        ? request.AssignedShelter.Name
                        : null),
                 AISummary = request.AISummary,
                 AIRecommendation = request.AIRecommendation,
                 AIConfidenceScore = request.AIConfidenceScore,
                AffectedPeopleCount = request.AffectedPeopleCount,
                Status = request.Status.ToString(),
                CreatedAtUtc = request.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return requests;
    }


    // =========================================================
    // GET ASSIGNED VOLUNTEER FOR CITIZEN REQUEST
    // =========================================================
    public async Task<AssignedVolunteerDto?>
        GetAssignedVolunteerForRequestAsync(
            Guid citizenId,
            Guid requestId,
            CancellationToken cancellationToken)
    {
        var request = await _context.AssistanceRequests
            .Include(r => r.AssignedVolunteer)
            .FirstOrDefaultAsync(
                r => r.Id == requestId && r.CitizenId == citizenId,
                cancellationToken);

        if (request == null || request.AssignedVolunteer == null)
        {
            return null;
        }

        var volunteer = request.AssignedVolunteer;

        return new AssignedVolunteerDto
        {
            VolunteerId = volunteer.Id,
            Name = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber ?? string.Empty,
            Email = volunteer.Email ?? string.Empty,
            Status = "En Route / Assigned",
            Skills = "General Emergency Response"
        };
    }


    // =========================================================
    // CREATE DIRECT SHELTER REQUEST
    // =========================================================
    public async Task<Guid> CreateShelterRequestAsync(
        Guid citizenId,
        Guid shelterId,
        CreateAssistanceRequest request,
        CancellationToken cancellationToken)
    {
        var shelter = await _context.Shelters
            .FirstOrDefaultAsync(
                s => s.Id == shelterId,
                cancellationToken);

        if (shelter == null)
        {
            throw new ArgumentException("Shelter not found.");
        }

        var citizenExists = await _context.Users
            .AnyAsync(u => u.Id == citizenId &&
                            u.Role == UserRole.Citizen &&
                            u.Status == UserStatus.Active,
                cancellationToken);

        if (!citizenExists)
        {
            throw new UnauthorizedAccessException(
                "Active citizen account not found.");
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
                "Affected people count must be at least 1.");
        }

        if (request.Latitude.HasValue &&
            (request.Latitude < -90 || request.Latitude > 90))
        {
            throw new ArgumentException("Invalid latitude.");
        }

        if (request.Longitude.HasValue &&
            (request.Longitude < -180 || request.Longitude > 180))
        {
            throw new ArgumentException("Invalid longitude.");
        }

        var shelterRequest = new AssistanceRequest
        {
            CitizenId = citizenId,
            Category = RequestCategory.Shelter,
            PriorityUrgency = RequestPriority.Medium,
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AffectedPeopleCount = request.Affected,
            Status = RequestStatus.Submitted,
            AssignedShelterId = shelter.Id,
            AssignedShelterName = shelter.Name
        };

        var analysis = await _aiAnalysisService.AnalyzeRequestAsync(
            new AnalyzeRequestDto
            {
                Category = RequestCategory.Shelter.ToString(),
                Urgency = string.IsNullOrWhiteSpace(request.Urgency)
                    ? RequestPriority.Medium.ToString()
                    : request.Urgency,
                Description = request.Description,
                Location = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Affected = request.Affected,
                Language = request.Language
            },
            cancellationToken);

        shelterRequest.AISummary = analysis.Summary;
        shelterRequest.AIRecommendation = analysis.RecommendedAction;
        shelterRequest.AIConfidenceScore = analysis.ConfidenceScore;

        _context.AssistanceRequests.Add(shelterRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return shelterRequest.Id;
    }


    // =========================================================
    // GET REQUESTS FOR A SPECIFIC SHELTER
    // =========================================================
    public async Task<IReadOnlyList<MyAssistanceRequestResponse>>
        GetShelterRequestsAsync(
            Guid shelterId,
            Guid shelterAdminId,
            CancellationToken cancellationToken)
    {
        var shelter = await _context.Shelters
            .FirstOrDefaultAsync(
                s => s.Id == shelterId,
                cancellationToken);

        if (shelter == null)
        {
            throw new ArgumentException("Shelter not found.");
        }

        // Check whether the user manages this shelter
        if (shelter.ShelterAdminId != shelterAdminId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to manage this shelter.");
        }

        var requests = await _context.AssistanceRequests
            .AsNoTracking()
            .Where(r => r.AssignedShelterId == shelterId &&
                        r.Category == RequestCategory.Shelter)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new MyAssistanceRequestResponse
            {
                Id = r.Id,
                Category = r.Category.ToString(),
                Priority = r.PriorityUrgency.ToString(),
                Description = r.Description,
                Location = r.Location,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                AssignedShelterName = r.AssignedShelterName,
                 AISummary = r.AISummary,
                 AIRecommendation = r.AIRecommendation,
                 AIConfidenceScore = r.AIConfidenceScore,
                AffectedPeopleCount = r.AffectedPeopleCount,
                Status = r.Status.ToString(),
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return requests;
    }


    // =========================================================
    // VERIFY SHELTER REQUEST
    // =========================================================
    public async Task<bool> VerifyShelterRequestAsync(
        Guid shelterId,
        Guid requestId,
        Guid shelterAdminId,
        CancellationToken cancellationToken)
    {
        var shelter = await _context.Shelters
            .FirstOrDefaultAsync(
                s => s.Id == shelterId,
                cancellationToken);

        if (shelter == null)
        {
            throw new ArgumentException("Shelter not found.");
        }

        // Check whether the user manages this shelter
        if (shelter.ShelterAdminId != shelterAdminId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to manage this shelter.");
        }

        var request = await _context.AssistanceRequests
            .FirstOrDefaultAsync(
                r => r.Id == requestId &&
                      r.AssignedShelterId == shelterId &&
                      r.Category == RequestCategory.Shelter,
                cancellationToken);

        if (request == null)
        {
            throw new ArgumentException("Shelter request not found.");
        }

        if (request.Status != RequestStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted requests can be verified.");
        }

        request.Status = RequestStatus.Verified;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}