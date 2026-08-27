using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CrearSolicitudCommand(CrearSolicitudRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class CrearSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<CrearSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        CrearSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.CreateAsync(command.Request, command.CurrentUser, cancellationToken);
}
