using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Notifications;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Infrastructure.Persistence;

namespace SB.Solicitudes.Infrastructure.Repositories;

internal sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public async Task AddAsync(Notificacion notification, CancellationToken cancellationToken) =>
        await dbContext.Notificaciones.AddAsync(notification, cancellationToken);

    public async Task<IReadOnlyCollection<NotificationItem>> GetLatestAsync(
        Guid recipientId,
        int limit,
        CancellationToken cancellationToken) =>
        await dbContext.Notificaciones.AsNoTracking()
            .Where(notification => notification.DestinatarioId == recipientId)
            .OrderByDescending(notification => notification.FechaCreacion)
            .Take(limit)
            .Select(notification => new NotificationItem(
                notification.Id,
                notification.SolicitudId,
                notification.Solicitud.Codigo,
                notification.Asunto,
                notification.Mensaje,
                notification.FechaCreacion,
                notification.FechaLectura != null,
                notification.FechaLectura))
            .ToListAsync(cancellationToken);

    public Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken) =>
        dbContext.Notificaciones.CountAsync(
            notification => notification.DestinatarioId == recipientId && notification.FechaLectura == null,
            cancellationToken);

    public Task<Notificacion?> GetForRecipientAsync(
        long id,
        Guid recipientId,
        CancellationToken cancellationToken) =>
        dbContext.Notificaciones.SingleOrDefaultAsync(
            notification => notification.Id == id && notification.DestinatarioId == recipientId,
            cancellationToken);

    public async Task<IReadOnlyCollection<Notificacion>> GetUnreadForRecipientAsync(
        Guid recipientId,
        CancellationToken cancellationToken) =>
        await dbContext.Notificaciones
            .Where(notification => notification.DestinatarioId == recipientId && notification.FechaLectura == null)
            .ToListAsync(cancellationToken);
}
