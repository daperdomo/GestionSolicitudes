using MediatR;

namespace SB.Solicitudes.Application.Notifications;

public sealed record ObtenerNotificacionesQuery(Guid RecipientId, int Limit = 20)
    : IRequest<IReadOnlyCollection<NotificationItem>>;

internal sealed class ObtenerNotificacionesQueryHandler(INotificationService service)
    : IRequestHandler<ObtenerNotificacionesQuery, IReadOnlyCollection<NotificationItem>>
{
    public Task<IReadOnlyCollection<NotificationItem>> Handle(
        ObtenerNotificacionesQuery query,
        CancellationToken cancellationToken) =>
        service.GetLatestAsync(query.RecipientId, query.Limit, cancellationToken);
}
