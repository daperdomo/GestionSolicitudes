using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CambiarPrioridadSolicitudCommand(long Id, CambiarPrioridadRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class CambiarPrioridadSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<CambiarPrioridadSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        CambiarPrioridadSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.ChangePriorityAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
