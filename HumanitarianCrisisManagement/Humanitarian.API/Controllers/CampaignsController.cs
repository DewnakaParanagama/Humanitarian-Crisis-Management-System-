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
                Title = c.Title,
                Description = c.Description,
                TargetAmount = c.TargetAmount,
                CurrentAmount = c.CurrentAmount,
                Status = c.Status,
                EndDate = c.EndDate
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
                Title = c.Title,
                Description = c.Description,
                TargetAmount = c.TargetAmount,
                CurrentAmount = c.CurrentAmount,
                Status = c.Status,
                EndDate = c.EndDate
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
            Title = dto.Title,
            Description = dto.Description,
            TargetAmount = dto.TargetAmount,
            CurrentAmount = dto.CurrentAmount,
            Status = dto.Status ?? "Active",
            EndDate = dto.EndDate
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CampaignResponseDto
        {
            Id = campaign.Id,
            Title = campaign.Title,
            Description = campaign.Description,
            TargetAmount = campaign.TargetAmount,
            CurrentAmount = campaign.CurrentAmount,
            Status = campaign.Status,
            EndDate = campaign.EndDate
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

        campaign.Title = dto.Title ?? campaign.Title;
        campaign.Description = dto.Description ?? campaign.Description;
        campaign.TargetAmount = dto.TargetAmount ?? campaign.TargetAmount;
        campaign.CurrentAmount = dto.CurrentAmount ?? campaign.CurrentAmount;
        campaign.Status = dto.Status ?? campaign.Status;
        campaign.EndDate = dto.EndDate ?? campaign.EndDate;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new CampaignResponseDto
        {
            Id = campaign.Id,
            Title = campaign.Title,
            Description = campaign.Description,
            TargetAmount = campaign.TargetAmount,
            CurrentAmount = campaign.CurrentAmount,
            Status = campaign.Status,
            EndDate = campaign.EndDate
        });
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