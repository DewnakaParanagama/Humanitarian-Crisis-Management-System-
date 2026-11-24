using Humanitarian.Application.Common.Interfaces;
using Humanitarian.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Humanitarian.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendDispatchAlertAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveDispatchAlert", message, cancellationToken);
    }
}