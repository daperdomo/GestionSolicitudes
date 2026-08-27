using Microsoft.AspNetCore.SignalR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Api.Notifications;

internal sealed class SignalRNotificationDispatcher(
    IHubContext<NotificationsHub> hubContext,
    ILogger<SignalRNotificationDispatcher> logger) : INotificationDispatcher
{
    private static readonly Action<ILogger, long, long, Guid, Exception?> LogNotification =
        LoggerMessage.Define<long, long, Guid>(
            LogLevel.Information,
            new EventId(1001, "NotificationDispatched"),
            "Notificación {NotificationId} de solicitud {RequestId} destinada a {RecipientId}");

    public async Task DispatchAsync(NotificationDispatchMessage message, CancellationToken cancellationToken)
    {
        LogNotification(logger, message.NotificationId, message.SolicitudId, message.RecipientId, null);
        await hubContext.Clients
            .Group(NotificationHubGroups.ForUser(message.RecipientId))
            .SendAsync(
                "notificationReceived",
                new
                {
                    Id = message.NotificationId,
                    message.SolicitudId,
                    message.CodigoSolicitud,
                    Asunto = message.Subject,
                    Mensaje = message.Message,
                    FechaCreacion = message.CreatedAt,
                },
                cancellationToken);
    }
}
