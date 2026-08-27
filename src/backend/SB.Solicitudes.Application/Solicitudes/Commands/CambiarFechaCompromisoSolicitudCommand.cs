using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CambiarFechaCompromisoSolicitudCommand(
    long Id,
    CambiarFechaCompromisoRequest Request,
    CurrentUser CurrentUser) : IRequest<Result<SolicitudDetail>>;

internal sealed class CambiarFechaCompromisoSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<CambiarFechaCompromisoSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        CambiarFechaCompromisoSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.ChangeDueDateAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
