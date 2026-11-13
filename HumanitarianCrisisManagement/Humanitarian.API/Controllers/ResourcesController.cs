using Humanitarian.Application.Features.Resources.DTOs;
using Humanitarian.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/resources")]
[Authorize(Roles = "Admin,Coordinator,ResourceMgr")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    // GET: api/v1/resources
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResourceResponseDto>>> GetAllResources(
        CancellationToken cancellationToken)
    {
        var resources = await _resourceService.GetAllResourcesAsync(
            cancellationToken);

        return Ok(resources);
    }

    // POST: api/v1/resources
    [HttpPost]
    public async Task<ActionResult<ResourceResponseDto>> CreateResource(
        [FromBody] CreateResourceDto request,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceService.CreateResourceAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAllResources),
            new { id = resource.Id },
            resource);
    }

    // PUT: api/v1/resources/{id}/dispatch
    [HttpPut("{id:guid}/dispatch")]
    public async Task<ActionResult<ResourceResponseDto>> DispatchResource(
        Guid id,
        [FromBody] DispatchResourceDto request,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceService.DispatchResourceAsync(
            id,
            request,
            cancellationToken);

        return Ok(resource);
    }
}