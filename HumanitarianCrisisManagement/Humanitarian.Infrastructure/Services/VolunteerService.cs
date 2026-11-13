using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Volunteers.DTOs;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Humanitarian.Infrastructure.Services;

public class VolunteerService : IVolunteerService
{
    private readonly IAppDbContext _context;

    public VolunteerService(IAppDbContext context)
    {
        _context = context;
    }

    // Register a new volunteer
    public async Task<VolunteerResponseDto> RegisterVolunteerAsync(
        Guid userId,
        CreateVolunteerDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role != UserRole.Citizen)
            throw new UnauthorizedAccessException(
                "Only citizens can register as volunteers.");

        var existingProfile = await _context.VolunteerProfiles
            .AnyAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (existingProfile)
            throw new InvalidOperationException(
                "Volunteer profile already exists.");

        if (string.IsNullOrWhiteSpace(request.Availability))
            throw new ArgumentException("Availability is required.");

        if (string.IsNullOrWhiteSpace(request.Location))
            throw new ArgumentException("Location is required.");

        if (request.HasVehicle &&
            string.IsNullOrWhiteSpace(request.VehicleType))
            throw new ArgumentException(
                "Vehicle type is required when the volunteer has a vehicle.");

        var volunteer = new VolunteerProfile
        {
            UserId = userId,
            Skills = request.Skills ?? new List<string>(),
            Availability = VolunteerAvailability.Available,
            AvailabilityDetails = request.Availability.Trim(),
            HasVehicle = request.HasVehicle,
            VehicleType = request.HasVehicle
                ? request.VehicleType?.Trim()
                : null,
            CurrentLocation = request.Location.Trim(),
            VerificationStatus = VolunteerVerificationStatus.Pending,
            TasksCompleted = 0
        };

        _context.VolunteerProfiles.Add(volunteer);

        await _context.SaveChangesAsync(cancellationToken);

        volunteer.User = user;

        return MapToResponse(volunteer);
    }

    // Apply to become a volunteer
    public async Task<VolunteerResponseDto> ApplyVolunteerAsync(
        Guid userId,
        ApplyVolunteerDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role != UserRole.Citizen)
            throw new UnauthorizedAccessException(
                "Only citizens can apply to become volunteers.");

        var existingProfile = await _context.VolunteerProfiles
            .AnyAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (existingProfile)
            throw new InvalidOperationException(
                "Volunteer application already exists.");

        if (string.IsNullOrWhiteSpace(request.District))
            throw new ArgumentException("District is required.");

        if (string.IsNullOrWhiteSpace(request.Availability))
            throw new ArgumentException("Availability is required.");

        var volunteer = new VolunteerProfile
        {
            UserId = userId,

            District = request.District.Trim(),

            AvailabilityDetails = request.Availability.Trim(),

            Availability = VolunteerAvailability.Offline,

            Skills = request.Skills ?? new List<string>(),

            Assets = request.Assets ?? new List<string>(),

            Experience = string.IsNullOrWhiteSpace(request.Experience)
                ? null
                : request.Experience.Trim(),

            VerificationStatus = VolunteerVerificationStatus.Pending,

            TasksCompleted = 0,

            HasVehicle = request.Assets?.Any(
                asset => asset.Contains(
                    "vehicle",
                    StringComparison.OrdinalIgnoreCase)) == true
        };

        _context.VolunteerProfiles.Add(volunteer);

        await _context.SaveChangesAsync(cancellationToken);

        volunteer.User = user;

        return MapToResponse(volunteer);
    }

    // Get all volunteers
    public async Task<IReadOnlyList<VolunteerResponseDto>>
        GetAllVolunteersAsync(CancellationToken cancellationToken)
    {
        var volunteers = await _context.VolunteerProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .OrderBy(profile => profile.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return volunteers.Select(MapToResponse).ToList();
    }

    // Update volunteer verification status
    public async Task<VolunteerResponseDto> UpdateVolunteerStatusAsync(
        Guid volunteerId,
        UpdateVolunteerStatusDto request,
        CancellationToken cancellationToken)
    {
        var volunteer = await _context.VolunteerProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(
                profile => profile.Id == volunteerId,
                cancellationToken);

        if (volunteer == null)
            throw new KeyNotFoundException("Volunteer not found.");

        var inputStatus = request.Status?.Trim();

        if (!Enum.TryParse<VolunteerVerificationStatus>(
                inputStatus,
                true,
                out var newStatus) ||
            !Enum.IsDefined(typeof(VolunteerVerificationStatus), newStatus))
        {
            throw new ArgumentException(
                "Invalid volunteer verification status.");
        }

        // Update volunteer verification status
        volunteer.VerificationStatus = newStatus;

        // Upgrade Citizen account to Volunteer role
        if (newStatus == VolunteerVerificationStatus.Verified)
        {
            if (volunteer.User == null)
                throw new InvalidOperationException(
                    "Volunteer user account not found.");

            if (volunteer.User.Role == UserRole.Citizen)
            {
                volunteer.User.Role = UserRole.Volunteer;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(volunteer);
    }

    // Dispatch a volunteer
    public async Task DispatchVolunteerAsync(
        Guid volunteerId,
        DispatchVolunteerDto request,
        CancellationToken cancellationToken)
    {
        var volunteer = await _context.VolunteerProfiles
            .FirstOrDefaultAsync(
                profile => profile.Id == volunteerId,
                cancellationToken);

        if (volunteer == null)
            throw new KeyNotFoundException("Volunteer not found.");

        if (volunteer.VerificationStatus !=
                VolunteerVerificationStatus.Verified &&
            volunteer.VerificationStatus !=
                VolunteerVerificationStatus.Active)
        {
            throw new InvalidOperationException(
                "Only verified volunteers can be dispatched.");
        }

        if (volunteer.Availability != VolunteerAvailability.Available)
            throw new InvalidOperationException(
                "Volunteer is not currently available.");

        if (request.TargetId == Guid.Empty)
            throw new ArgumentException("Invalid target ID.");

        var targetType = request.TargetType?.Trim();

        if (string.Equals(
                targetType,
                "AssistanceRequest",
                StringComparison.OrdinalIgnoreCase))
        {
            var targetExists = await _context.AssistanceRequests
                .AnyAsync(
                    item => item.Id == request.TargetId,
                    cancellationToken);

            if (!targetExists)
                throw new KeyNotFoundException(
                    "Assistance request not found.");
        }
        else if (string.Equals(
                     targetType,
                     "Shelter",
                     StringComparison.OrdinalIgnoreCase))
        {
            var targetExists = await _context.Shelters
                .AnyAsync(
                    item => item.Id == request.TargetId,
                    cancellationToken);

            if (!targetExists)
                throw new KeyNotFoundException("Shelter not found.");
        }
        else
        {
            throw new ArgumentException(
                "Target type must be AssistanceRequest or Shelter.");
        }

        var assignment = new VolunteerAssignment
        {
            VolunteerProfileId = volunteerId,
            TargetId = request.TargetId,
            TargetType = targetType!,
            Status = "Assigned",
            DispatchedAtUtc = DateTime.UtcNow
        };

        _context.VolunteerAssignments.Add(assignment);

        volunteer.Availability = VolunteerAvailability.Busy;
        volunteer.VerificationStatus = VolunteerVerificationStatus.Active;

        await _context.SaveChangesAsync(cancellationToken);

    }

    // Get logged-in volunteer profile
    public async Task<MyVolunteerProfileDto> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var volunteer = await _context.VolunteerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (volunteer == null)
            throw new KeyNotFoundException(
                "Volunteer profile not found.");

        return new MyVolunteerProfileDto
        {
            Id = volunteer.Id,
            VerificationStatus = volunteer.VerificationStatus.ToString(),
            Skills = volunteer.Skills,
            CompletedTasks = volunteer.TasksCompleted
        };
    }

    // Get tasks assigned to logged-in volunteer
    public async Task<IReadOnlyList<VolunteerTaskResponseDto>>
        GetMyTasksAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        var volunteer = await _context.VolunteerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (volunteer == null)
            throw new KeyNotFoundException(
                "Volunteer profile not found.");

        var assignments = await _context.VolunteerAssignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.VolunteerProfileId == volunteer.Id)
            .OrderByDescending(assignment => assignment.DispatchedAtUtc)
            .ToListAsync(cancellationToken);

        var tasks = new List<VolunteerTaskResponseDto>();

        foreach (var assignment in assignments)
        {
            string location = string.Empty;

            if (string.Equals(
                    assignment.TargetType,
                    "AssistanceRequest",
                    StringComparison.OrdinalIgnoreCase))
            {
                var target = await _context.AssistanceRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        item => item.Id == assignment.TargetId,
                        cancellationToken);

                location = target?.Location ?? string.Empty;
            }
            else if (string.Equals(
                         assignment.TargetType,
                         "Shelter",
                         StringComparison.OrdinalIgnoreCase))
            {
                var target = await _context.Shelters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        item => item.Id == assignment.TargetId,
                        cancellationToken);

                location = target?.Location ?? string.Empty;
            }

            tasks.Add(new VolunteerTaskResponseDto
            {
                Id = assignment.Id,
                TargetType = assignment.TargetType,
                TargetId = assignment.TargetId,
                Location = location,
                Status = assignment.Status,
                AssignedAt = assignment.DispatchedAtUtc
            });
        }

        return tasks;
    }

    // Update assigned task status
    public async Task UpdateTaskStatusAsync(
        Guid userId,
        Guid taskId,
        UpdateVolunteerTaskStatusDto request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                request.Status?.Trim(),
                "Completed",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only Completed status is supported.");
        }

        var volunteer = await _context.VolunteerProfiles
            .FirstOrDefaultAsync(
                profile => profile.UserId == userId,
                cancellationToken);

        if (volunteer == null)
            throw new KeyNotFoundException(
                "Volunteer profile not found.");

        await using var transaction =
            await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var assignment = await _context.VolunteerAssignments
                .FirstOrDefaultAsync(
                    item => item.Id == taskId &&
                            item.VolunteerProfileId == volunteer.Id,
                    cancellationToken);

            if (assignment == null)
                throw new KeyNotFoundException(
                    "Assigned task not found.");

            if (!string.Equals(
                    assignment.Status,
                    "Assigned",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This task has already been completed or is not assigned.");
            }

            // 1. Mark task as completed
            assignment.Status = "Completed";

            // 2. Increment completed task count
            volunteer.TasksCompleted++;

            // 3. Make volunteer available again
            volunteer.Availability = VolunteerAvailability.Available;
            volunteer.VerificationStatus =
                VolunteerVerificationStatus.Verified;

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // Map entity to response DTO
    private static VolunteerResponseDto MapToResponse(
        VolunteerProfile volunteer)
    {
        return new VolunteerResponseDto
        {
            Id = volunteer.Id,
            UserId = volunteer.UserId,
            FullName = volunteer.User?.FullName ?? "Unknown",
            Email = volunteer.User?.Email ?? string.Empty,
            PhoneNumber = volunteer.User?.PhoneNumber ?? string.Empty,
            Skills = volunteer.Skills,
            Availability = volunteer.AvailabilityDetails,
            HasVehicle = volunteer.HasVehicle,
            VehicleType = volunteer.VehicleType,
            Location = volunteer.CurrentLocation ?? string.Empty,
            VerificationStatus = volunteer.VerificationStatus.ToString(),
            TasksCompleted = volunteer.TasksCompleted
        };
    }
    // Update live location
    public async Task UpdateLiveLocationAsync(Guid userId, UpdateVolunteerLocationDto request, CancellationToken cancellationToken)
    {
        var volunteer = await _context.VolunteerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (volunteer == null)
            throw new KeyNotFoundException("Volunteer profile not found.");

        volunteer.Latitude = request.Latitude;
        volunteer.Longitude = request.Longitude;
        volunteer.LocationLastUpdatedUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Get active locations for map
    public async Task<IReadOnlyList<ActiveVolunteerLocationDto>> GetActiveLocationsAsync
        (
        CancellationToken cancellationToken
        )
    {
        var activeVolunteers = await _context.VolunteerProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.VerificationStatus == VolunteerVerificationStatus.Active
                     && p.Latitude.HasValue
                     && p.Longitude.HasValue)
            .ToListAsync(cancellationToken);

        return activeVolunteers.Select(v => new ActiveVolunteerLocationDto
        {
            VolunteerId = v.Id,
            FullName = v.User?.FullName ?? "Unknown Volunteer",
            Latitude = v.Latitude!.Value,
            Longitude = v.Longitude!.Value,
            LastUpdatedUtc = v.LocationLastUpdatedUtc!.Value
        }).ToList();
    }
}