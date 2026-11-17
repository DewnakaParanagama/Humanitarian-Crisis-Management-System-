using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Campaigns.DTOs;
using Humanitarian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly IAppDbContext _context;

    public CampaignsController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignResponseDto>>> GetCampaigns(CancellationToken cancellationToken)
    {
        return await _context.Campaigns
            .Select(c => new CampaignResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                TargetAmount = c.TargetAmount,
                RaisedAmount = c.RaisedAmount,
                Status = c.Status
            })
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignResponseDto>> GetCampaignById(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .Where(c => c.Id == id)
            .Select(c => new CampaignResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                TargetAmount = c.TargetAmount,
                RaisedAmount = c.RaisedAmount,
                Status = c.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign == null)
        {
            return NotFound();
        }

        return campaign;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<CampaignResponseDto>> CreateCampaign([FromBody] CreateCampaignDto dto, CancellationToken cancellationToken)
    {
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            TargetAmount = dto.TargetAmount,
            RaisedAmount = 0m,
            Status = dto.Status ?? "Active"
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CampaignResponseDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            TargetAmount = campaign.TargetAmount,
            RaisedAmount = campaign.RaisedAmount,
            Status = campaign.Status
        };

        return CreatedAtAction(nameof(GetCampaignById), new { id = campaign.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<IActionResult> UpdateCampaign(Guid id, [FromBody] UpdateCampaignDto dto, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
        {
            return NotFound();
        }

        campaign.Name = dto.Name ?? campaign.Name;
        campaign.TargetAmount = dto.TargetAmount != 0 ? dto.TargetAmount : campaign.TargetAmount;
        campaign.Status = dto.Status ?? campaign.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<IActionResult> DeleteCampaign(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
        {
            return NotFound();
        }

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}