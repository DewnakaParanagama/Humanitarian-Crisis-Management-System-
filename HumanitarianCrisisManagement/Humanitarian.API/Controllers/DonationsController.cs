using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Donations.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/donations")]
public class DonationsController : ControllerBase
{
    private readonly IAppDbContext _context;

    public DonationsController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DonationResponseDto>>> GetDonations(CancellationToken cancellationToken)
    {
        return await _context.Donations
            .Select(d => new DonationResponseDto
            {
                Id = d.Id,
                DonorName = string.IsNullOrEmpty(d.DonorName) ? "Anonymous" : d.DonorName,
                Type = d.Type.ToString(),
                AmountOrValue = d.AmountOrValue,
                CampaignName = d.CampaignName ?? "General",
                Status = d.Status.ToString(),
                Date = d.DateReceivedUtc
            })
            .ToListAsync(cancellationToken);
    }
}