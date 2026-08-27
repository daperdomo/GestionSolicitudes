using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CambiarTipoSolicitudCommand(long Id, CambiarTipoSolicitudRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class CambiarTipoSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<CambiarTipoSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        CambiarTipoSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.ChangeRequestTypeAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
