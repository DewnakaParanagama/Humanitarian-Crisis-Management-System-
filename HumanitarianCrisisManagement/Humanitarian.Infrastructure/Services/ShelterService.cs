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

        return shelters.Select(shelter => new ShelterResponseDto
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
            Organization = shelter.Organization
        }).ToList();
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
            ShelterAdminId = shelterAdminId
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
            Organization = shelter.Organization
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
                shelter => shelter.Id == shelterId,
                cancellationToken);

        if (shelter == null)
            throw new KeyNotFoundException("Shelter not found.");

        if (shelter.Status == ShelterStatus.Closed)
            throw new InvalidOperationException("Cannot allocate people to a closed shelter.");

        if (shelter.CurrentOccupancy + request.Count > shelter.TotalCapacity)
            throw new InvalidOperationException("Shelter capacity would be exceeded.");

        shelter.CurrentOccupancy += request.Count;

        if (shelter.CurrentOccupancy >= shelter.TotalCapacity)
            shelter.Status = ShelterStatus.Full;

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
            Organization = shelter.Organization
        };
    }
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