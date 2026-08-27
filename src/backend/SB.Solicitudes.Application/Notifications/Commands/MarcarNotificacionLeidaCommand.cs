using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Notifications;

public sealed record MarcarNotificacionLeidaCommand(long Id, Guid RecipientId) : IRequest<Result<bool>>;

internal sealed class MarcarNotificacionLeidaCommandHandler(INotificationService service)
    : IRequestHandler<MarcarNotificacionLeidaCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(
        MarcarNotificacionLeidaCommand command,
        CancellationToken cancellationToken) =>
        service.MarkAsReadAsync(command.Id, command.RecipientId, cancellationToken);
}
