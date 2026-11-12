using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Campaigns.DTOs;
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
}