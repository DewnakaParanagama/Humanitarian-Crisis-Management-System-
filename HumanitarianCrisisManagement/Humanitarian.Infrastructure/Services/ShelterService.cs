using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Shelters.DTOs;
using Humanitarian.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class ShelterService : IShelterService
{
    private readonly IAppDbContext _context;

    public ShelterService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NearbyShelterResponse>>
        GetNearbySheltersAsync(
            double latitude,
            double longitude,
            double radiusKm,
            CancellationToken cancellationToken)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Invalid latitude.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Invalid longitude.");

        if (radiusKm <= 0 || radiusKm > 100)
            throw new ArgumentException("Invalid search radius.");

        var shelters = await _context.Shelters
            .AsNoTracking()
            .Where(shelter =>
                shelter.Latitude != null &&
                shelter.Longitude != null &&
                shelter.Status == ShelterStatus.Open &&
                shelter.CurrentOccupancy < shelter.TotalCapacity)
            .ToListAsync(cancellationToken);

        return shelters
            .Select(shelter =>
            {
                var distance = CalculateDistance(
                    latitude,
                    longitude,
                    shelter.Latitude!.Value,
                    shelter.Longitude!.Value);

                return new NearbyShelterResponse
                {
                    Id = shelter.Id,
                    Name = shelter.Name,
                    Location = shelter.Location,
                    Latitude = shelter.Latitude.Value,
                    Longitude = shelter.Longitude.Value,
                    TotalCapacity = shelter.TotalCapacity,
                    CurrentOccupancy = shelter.CurrentOccupancy,
                    ContactNumber = shelter.Contact ?? string.Empty,
                    Facilities = shelter.Facilities,
                    Status = shelter.Status.ToString(),
                    DistanceKm = Math.Round(distance, 2)
                };
            })
            .Where(shelter => shelter.DistanceKm <= radiusKm)
            .OrderBy(shelter => shelter.DistanceKm)
            .ToList();
    }

    public async Task<IReadOnlyList<ShelterResponseDto>>
        GetAllSheltersAsync(CancellationToken cancellationToken)
    {
        var shelters = await _context.Shelters
            .AsNoTracking()
            .OrderBy(shelter => shelter.Name)
            .ToListAsync(cancellationToken);

        var result = new List<ShelterResponseDto>();

        foreach (var shelter in shelters)
        {
            var manager = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    user => user.Id == shelter.ShelterAdminId,
                    cancellationToken);

            result.Add(new ShelterResponseDto
            {
                Id = shelter.Id,
                Name = shelter.Name,
                Location = shelter.Location,
                District = shelter.District,
                Latitude = shelter.Latitude,
                Longitude = shelter.Longitude,
                Capacity = shelter.TotalCapacity,
                Occupied = shelter.CurrentOccupancy,
                Status = shelter.Status.ToString(),
                Facilities = shelter.Facilities,
                Accessible = shelter.Accessible,
                Contact = shelter.Contact,
                Organization = shelter.Organization,
                ManagerId = manager?.Id,
                ManagerName = manager?.FullName
            });
        }

        return result;
    }

    public async Task<ShelterResponseDto> CreateShelterAsync(
        CreateShelterDto request,
        Guid shelterAdminId,
        CancellationToken cancellationToken)
    {
        if (request.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.");

        if (request.Latitude < -90 || request.Latitude > 90)
            throw new ArgumentException("Invalid latitude.");

        if (request.Longitude < -180 || request.Longitude > 180)
            throw new ArgumentException("Invalid longitude.");

        var volunteerProfile = await _context.VolunteerProfiles
            .FirstOrDefaultAsync(
                v => v.Id == request.ManagerVolunteerId ||
                      v.UserId == request.ManagerVolunteerId,
                cancellationToken);

        if (volunteerProfile == null)
            throw new KeyNotFoundException("Volunteer profile not found.");

        if (volunteerProfile.VerificationStatus != VolunteerVerificationStatus.Verified)
            throw new InvalidOperationException(
                "Only verified volunteers can be assigned as shelter managers.");

        var managerUser = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == volunteerProfile.UserId,
                cancellationToken);

        if (managerUser == null)
            throw new KeyNotFoundException("Volunteer user account not found.");

        managerUser.Role = UserRole.ShelterAdmin;

        var shelter = new Humanitarian.Domain.Entities.Shelter
        {
            Name = request.Name,
            Location = request.Location,
            District = request.District,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TotalCapacity = request.Capacity,
            CurrentOccupancy = 0,
            Contact = request.Contact,
            Organization = request.Organization,
            Accessible = request.Accessible,
            Facilities = request.Facilities,
            Status = ShelterStatus.Open,
            ShelterAdminId = managerUser.Id
        };

        _context.Shelters.Add(shelter);

        await _context.SaveChangesAsync(cancellationToken);

        return new ShelterResponseDto
        {
            Id = shelter.Id,
            Name = shelter.Name,
            Location = shelter.Location,
            District = shelter.District,
            Latitude = shelter.Latitude,
            Longitude = shelter.Longitude,
            Capacity = shelter.TotalCapacity,
            Occupied = shelter.CurrentOccupancy,
            Status = shelter.Status.ToString(),
            Facilities = shelter.Facilities,
            Accessible = shelter.Accessible,
            Contact = shelter.Contact,
            Organization = shelter.Organization,
            ManagerId = managerUser.Id,
            ManagerName = managerUser.FullName
        };
    }

    public async Task<ShelterResponseDto> AllocatePeopleAsync(
        Guid shelterId,
        AllocateShelterDto request,
        CancellationToken cancellationToken)
    {
        if (request.Count <= 0)
            throw new ArgumentException("Allocation count must be greater than zero.");

        var shelter = await _context.Shelters
            .FirstOrDefaultAsync(
                s => s.Id == shelterId,
                cancellationToken);

        if (shelter == null)
            throw new KeyNotFoundException("Shelter not found.");

        if (shelter.Status == ShelterStatus.Closed)
            throw new InvalidOperationException(
                "Cannot allocate people to a closed shelter.");

        if (shelter.CurrentOccupancy + request.Count > shelter.TotalCapacity)
            throw new InvalidOperationException(
                "Shelter capacity would be exceeded.");

        if (request.RequestId.HasValue &&
            request.RequestId.Value != Guid.Empty)
        {
            var assistanceRequest = await _context.AssistanceRequests
                .FirstOrDefaultAsync(
                    r => r.Id == request.RequestId.Value,
                    cancellationToken);

            if (assistanceRequest == null)
                throw new KeyNotFoundException(
                    "Assistance request not found.");

            assistanceRequest.AssignedShelterId = shelter.Id;
            assistanceRequest.AssignedShelterName = shelter.Name;
        }

        shelter.CurrentOccupancy += request.Count;

        if (shelter.CurrentOccupancy >= shelter.TotalCapacity)
            shelter.Status = ShelterStatus.Full;

        await _context.SaveChangesAsync(cancellationToken);

        var manager = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Id == shelter.ShelterAdminId,
                cancellationToken);

        return new ShelterResponseDto
        {
            Id = shelter.Id,
            Name = shelter.Name,
            Location = shelter.Location,
            District = shelter.District,
            Latitude = shelter.Latitude,
            Longitude = shelter.Longitude,
            Capacity = shelter.TotalCapacity,
            Occupied = shelter.CurrentOccupancy,
            Status = shelter.Status.ToString(),
            Facilities = shelter.Facilities,
            Accessible = shelter.Accessible,
            Contact = shelter.Contact,
            Organization = shelter.Organization,
            ManagerId = manager?.Id,
            ManagerName = manager?.FullName
        };
    }

    // ==========================================
    // SHELTER ROSTER
    // ==========================================

    public async Task<ShelterRosterDto> GetShelterRosterAsync(
        Guid shelterId,
        Guid shelterAdminId,
        CancellationToken cancellationToken)
    {
        var shelter = await _context.Shelters
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == shelterId,
                cancellationToken);

        if (shelter == null)
            throw new KeyNotFoundException("Shelter not found.");

        if (shelter.ShelterAdminId != shelterAdminId)
            throw new UnauthorizedAccessException(
                "You are not authorized to manage this shelter.");

        // Get citizens assigned to this shelter
        var citizenIds = await _context.AssistanceRequests
            .AsNoTracking()
            .Where(r =>
                r.AssignedShelterId == shelterId &&
                r.Category == RequestCategory.Shelter &&
                (r.Status == RequestStatus.Verified ||
                 r.Status == RequestStatus.Assigned ||
                 r.Status == RequestStatus.InProgress ||
                 r.Status == RequestStatus.Delivered))
            .Select(r => r.CitizenId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var citizens = await _context.Users
            .AsNoTracking()
            .Where(u => citizenIds.Contains(u.Id))
            .Select(u => new ShelterRosterMemberDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role.ToString(),
                Status = u.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Get volunteers assigned to this shelter
        var volunteerProfileIds = await _context.VolunteerAssignments
            .AsNoTracking()
            .Where(a =>
                a.TargetId == shelterId &&
                a.TargetType == "Shelter" &&
                a.Status == "Assigned")
            .Select(a => a.VolunteerProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var volunteerUserIds = await _context.VolunteerProfiles
            .AsNoTracking()
            .Where(v => volunteerProfileIds.Contains(v.Id))
            .Select(v => v.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var volunteers = await _context.Users
            .AsNoTracking()
            .Where(u => volunteerUserIds.Contains(u.Id))
            .Select(u => new ShelterRosterMemberDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role.ToString(),
                Status = u.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new ShelterRosterDto
        {
            Citizens = citizens,
            Volunteers = volunteers
        };
    }

    // ==========================================
    // DISTANCE CALCULATIONS
    // ==========================================

    private static double CalculateDistance(
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(lat1)) *
            Math.Cos(DegreesToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}