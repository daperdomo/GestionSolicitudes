using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Infrastructure.Persistence;

namespace SB.Solicitudes.Infrastructure.Repositories;

internal sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public async Task AddAsync(Notificacion notification, CancellationToken cancellationToken) =>
        await dbContext.Notificaciones.AddAsync(notification, cancellationToken);
}
