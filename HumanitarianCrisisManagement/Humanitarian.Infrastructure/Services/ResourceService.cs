using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Resources.DTOs;
using Humanitarian.Application.Interfaces;
using Humanitarian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.Infrastructure.Services;

public class ResourceService : IResourceService
{
    private readonly IAppDbContext _context;

    public ResourceService(IAppDbContext context)
    {
        _context = context;
    }

    // Get all resources
    public async Task<IReadOnlyList<ResourceResponseDto>> GetAllResourcesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Resources
            .AsNoTracking()
            .Select(resource => new ResourceResponseDto
            {
                Id = resource.Id,
                ItemName = resource.ItemName,
                Category = resource.Category,
                Quantity = resource.Quantity,
                Unit = resource.Unit,
                Location = resource.Location,
                Status = resource.Status,
                LastUpdated = resource.UpdatedAtUtc ?? resource.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    // Create a new resource
    public async Task<ResourceResponseDto> CreateResourceAsync(
        CreateResourceDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
            throw new ArgumentException("Item name is required.");

        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Category is required.");

        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Unit))
            throw new ArgumentException("Unit is required.");

        if (string.IsNullOrWhiteSpace(request.Location))
            throw new ArgumentException("Location is required.");

        var resource = new Resource
        {
            ItemName = request.ItemName.Trim(),
            Category = request.Category.Trim(),
            Quantity = request.Quantity,
            Unit = request.Unit.Trim(),
            Location = request.Location.Trim(),
            Status = "Available"
        };

        _context.Resources.Add(resource);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(resource);
    }

    // Dispatch resources
    public async Task<ResourceResponseDto> DispatchResourceAsync(
        Guid id,
        DispatchResourceDto request,
        CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (resource == null)
            throw new KeyNotFoundException("Resource not found.");

        if (request.DispatchQuantity <= 0)
            throw new ArgumentException("Dispatch quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Destination))
            throw new ArgumentException("Destination is required.");

        if (request.DispatchQuantity > resource.Quantity)
            throw new InvalidOperationException("Insufficient resource quantity.");

        resource.Quantity -= request.DispatchQuantity;

        resource.Status = resource.Quantity == 0
            ? "OutOfStock"
            : "Available";

        resource.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(resource);
    }

    private static ResourceResponseDto MapToResponse(Resource resource)
    {
        return new ResourceResponseDto
        {
            Id = resource.Id,
            ItemName = resource.ItemName,
            Category = resource.Category,
            Quantity = resource.Quantity,
            Unit = resource.Unit,
            Location = resource.Location,
            Status = resource.Status,
            LastUpdated = resource.UpdatedAtUtc ?? resource.CreatedAtUtc
        };
    }
}