using MediatR;

namespace SB.Solicitudes.Application.Notifications;

public sealed record MarcarTodasNotificacionesLeidasCommand(Guid RecipientId) : IRequest<int>;

internal sealed class MarcarTodasNotificacionesLeidasCommandHandler(INotificationService service)
    : IRequestHandler<MarcarTodasNotificacionesLeidasCommand, int>
{
    public Task<int> Handle(
        MarcarTodasNotificacionesLeidasCommand command,
        CancellationToken cancellationToken) =>
        service.MarkAllAsReadAsync(command.RecipientId, cancellationToken);
}
