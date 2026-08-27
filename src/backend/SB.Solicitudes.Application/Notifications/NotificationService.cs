using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;

namespace SB.Solicitudes.Application.Notifications;

internal sealed class NotificationService(
    INotificationRepository repository,
    IUnitOfWork unitOfWork) : INotificationService
{
    public Task<IReadOnlyCollection<NotificationItem>> GetLatestAsync(
        Guid recipientId,
        int limit,
        CancellationToken cancellationToken) =>
        repository.GetLatestAsync(recipientId, Math.Clamp(limit, 1, 50), cancellationToken);

    public async Task<UnreadNotificationCount> CountUnreadAsync(
        Guid recipientId,
        CancellationToken cancellationToken) =>
        new(await repository.CountUnreadAsync(recipientId, cancellationToken));

    public async Task<Result<bool>> MarkAsReadAsync(
        long id,
        Guid recipientId,
        CancellationToken cancellationToken)
    {
        Notificacion? notification = await repository.GetForRecipientAsync(id, recipientId, cancellationToken);
        if (notification is null)
        {
            return Result<bool>.Failure(
                ErrorType.NotFound,
                "notification_not_found",
                "La notificación no existe o no pertenece al usuario actual.");
        }

        notification.MarcarComoLeida(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<int> MarkAllAsReadAsync(Guid recipientId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Notificacion> notifications =
            await repository.GetUnreadForRecipientAsync(recipientId, cancellationToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (Notificacion notification in notifications)
        {
            notification.MarcarComoLeida(now);
        }

        if (notifications.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return notifications.Count;
    }
}
