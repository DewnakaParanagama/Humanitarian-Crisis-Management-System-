namespace Humanitarian.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendDispatchAlertAsync(Guid userId, string message, CancellationToken cancellationToken);
}