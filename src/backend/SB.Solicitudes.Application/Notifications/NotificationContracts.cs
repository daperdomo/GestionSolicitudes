using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Notifications;

public sealed record NotificationItem(
    long Id,
    long SolicitudId,
    string CodigoSolicitud,
    string Asunto,
    string Mensaje,
    DateTimeOffset FechaCreacion,
    bool Leida,
    DateTimeOffset? FechaLectura);

public sealed record UnreadNotificationCount(int Total);

public interface INotificationService
{
    Task<IReadOnlyCollection<NotificationItem>> GetLatestAsync(
        Guid recipientId,
        int limit,
        CancellationToken cancellationToken);

    Task<UnreadNotificationCount> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken);

    Task<Result<bool>> MarkAsReadAsync(
        long id,
        Guid recipientId,
        CancellationToken cancellationToken);

    Task<int> MarkAllAsReadAsync(Guid recipientId, CancellationToken cancellationToken);
}
