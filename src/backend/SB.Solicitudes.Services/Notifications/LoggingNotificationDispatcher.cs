using Microsoft.Extensions.Logging;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;

namespace SB.Solicitudes.Services.Notifications;

internal sealed class LoggingNotificationDispatcher(
    ILogger<LoggingNotificationDispatcher> logger) : INotificationDispatcher
{
    private static readonly Action<ILogger, long, long, Guid, Exception?> LogNotification =
        LoggerMessage.Define<long, long, Guid>(
            LogLevel.Information,
            new EventId(1001, "NotificationDispatched"),
            "Notificación {NotificationId} de solicitud {RequestId} destinada a {RecipientId}");

    public Task DispatchAsync(Notificacion notification, CancellationToken cancellationToken)
    {
        LogNotification(
            logger,
            notification.Id,
            notification.SolicitudId,
            notification.DestinatarioId,
            null);

        return Task.CompletedTask;
    }
}
