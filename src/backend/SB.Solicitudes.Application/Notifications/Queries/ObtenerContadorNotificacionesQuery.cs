using MediatR;

namespace SB.Solicitudes.Application.Notifications;

public sealed record ObtenerContadorNotificacionesQuery(Guid RecipientId)
    : IRequest<UnreadNotificationCount>;

internal sealed class ObtenerContadorNotificacionesQueryHandler(INotificationService service)
    : IRequestHandler<ObtenerContadorNotificacionesQuery, UnreadNotificationCount>
{
    public Task<UnreadNotificationCount> Handle(
        ObtenerContadorNotificacionesQuery query,
        CancellationToken cancellationToken) =>
        service.CountUnreadAsync(query.RecipientId, cancellationToken);
}
