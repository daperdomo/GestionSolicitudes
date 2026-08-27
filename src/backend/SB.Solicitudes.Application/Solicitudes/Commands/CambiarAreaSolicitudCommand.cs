using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CambiarAreaSolicitudCommand(long Id, CambiarAreaRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class CambiarAreaSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<CambiarAreaSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        CambiarAreaSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.ChangeAreaAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
