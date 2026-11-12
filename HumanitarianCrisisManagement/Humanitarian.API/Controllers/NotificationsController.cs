using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Notifications.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IAppDbContext _context;

    public NotificationsController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications(CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.Timestamp)
            .Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                Timestamp = n.Timestamp,
                IsRead = n.IsRead
            })
            .ToListAsync(cancellationToken);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FindAsync(new object[] { id }, cancellationToken);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
