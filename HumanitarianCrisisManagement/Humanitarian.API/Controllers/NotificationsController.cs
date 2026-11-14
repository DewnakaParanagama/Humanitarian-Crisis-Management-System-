using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.Notifications.DTOs;
using Humanitarian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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

    // POST: api/v1/notifications (Restricted to Admin and Coordinator roles)
    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<NotificationResponseDto>> CreateNotification([FromBody] CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new NotificationResponseDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Timestamp = notification.Timestamp,
            IsRead = notification.IsRead
        };

        return CreatedAtAction(nameof(GetNotifications), new { id = notification.Id }, response);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
            
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