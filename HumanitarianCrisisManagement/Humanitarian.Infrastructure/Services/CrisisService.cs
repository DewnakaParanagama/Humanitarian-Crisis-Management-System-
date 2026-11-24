using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Crises.DTOs;
using Humanitarian.Domain.Entities;
using Humanitarian.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class CrisisService : ICrisisService
{
    private readonly IAppDbContext _context;

    public CrisisService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CrisisResponseDto>> GetAllCrisesAsync(
        CancellationToken cancellationToken)
    {
        var crises = await _context.Crises
            .AsNoTracking()
            .Select(crisis => new CrisisResponseDto
            {
                Id = crisis.Id,
                Title = crisis.Name,
                Type = crisis.Type.ToString(),
                Status = crisis.Status.ToString(),
                Location = crisis.Location,
                AffectedCount = crisis.AffectedPopulation,
                Latitude = crisis.Latitude,
                Longitude = crisis.Longitude,

                RequestsCount = _context.AssistanceRequests
                    .Count(request => request.CrisisId == crisis.Id)
            })
            .ToListAsync(cancellationToken);

        return crises;
    }

    public async Task<CrisisResponseDto> CreateCrisisAsync(
        CreateCrisisDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Crisis title is required.");

        if (string.IsNullOrWhiteSpace(request.Location))
            throw new ArgumentException("Crisis location is required.");

        if (!Enum.TryParse<CrisisType>(
            request.Type, true, out var crisisType))
            throw new ArgumentException("Invalid crisis type.");

        if (!Enum.TryParse<CrisisStatus>(
            request.Status, true, out var crisisStatus))
            throw new ArgumentException("Invalid crisis status.");

        if (request.AffectedCount < 0)
            throw new ArgumentException("Affected count cannot be negative.");

        if (request.Latitude < -90 || request.Latitude > 90)
            throw new ArgumentException("Invalid latitude.");

        if (request.Longitude < -180 || request.Longitude > 180)
            throw new ArgumentException("Invalid longitude.");

        var crisis = new Crisis
        {
            Name = request.Title.Trim(),
            Type = crisisType,
            Status = crisisStatus,
            Severity = CrisisSeverity.Medium,
            Location = request.Location.Trim(),
            AffectedPopulation = request.AffectedCount,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            StartDateUtc = DateTime.UtcNow
        };

        _context.Crises.Add(crisis);

        await _context.SaveChangesAsync(cancellationToken);

        return new CrisisResponseDto
        {
            Id = crisis.Id,
            Title = crisis.Name,
            Type = crisis.Type.ToString(),
            Status = crisis.Status.ToString(),
            Location = crisis.Location,
            AffectedCount = crisis.AffectedPopulation,
            Latitude = crisis.Latitude,
            Longitude = crisis.Longitude,
            RequestsCount = 0
        };
    }
}