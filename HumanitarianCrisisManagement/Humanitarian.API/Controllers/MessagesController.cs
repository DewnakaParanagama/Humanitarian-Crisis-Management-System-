using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Humanitarian.API.Controllers;

[ApiController]
[Route("api/v1/messages")]
public class MessagesController : ControllerBase
{
    private readonly IAppDbContext _context;

    public MessagesController(IAppDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/messages/request/{requestId}
    [HttpGet("request/{requestId:guid}")]
    public async Task<IActionResult> GetMessagesForRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var messages = await _context.DirectMessages
            .Where(m => m.RequestId == requestId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Id,
                m.RequestId,
                m.SenderId,
                m.ReceiverId,
                m.MessageBody,
                m.Timestamp
            })
            .ToListAsync(cancellationToken);

        return Ok(messages);
    }

    // POST: api/v1/messages/send
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var senderId))
        {
            // Fallback or handle unauthenticated simulation if auth tokens use strings/guids differently
            senderId = Guid.NewGuid(); 
        }

        var message = new DirectMessage
        {
            Id = Guid.NewGuid(),
            RequestId = dto.RequestId,
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            MessageBody = dto.MessageBody,
            Timestamp = DateTime.UtcNow
        };

        _context.DirectMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message.Id,
            message.RequestId,
            message.SenderId,
            message.ReceiverId,
            message.MessageBody,
            message.Timestamp
        });
    }
}

public class SendMessageDto
{
    public Guid RequestId { get; set; }
    public Guid ReceiverId { get; set; }
    public string MessageBody { get; set; } = string.Empty;
}