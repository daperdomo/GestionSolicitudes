using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CambiarEstadoSolicitudCommand(long Id, CambiarEstadoRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class CambiarEstadoSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<CambiarEstadoSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        CambiarEstadoSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.ChangeStatusAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
