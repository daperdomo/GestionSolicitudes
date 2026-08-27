using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record AsignarSolicitudCommand(long Id, AsignarSolicitudRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class AsignarSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<AsignarSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        AsignarSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.AssignAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
