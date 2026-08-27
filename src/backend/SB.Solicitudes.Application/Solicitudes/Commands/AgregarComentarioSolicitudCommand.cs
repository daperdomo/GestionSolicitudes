using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record AgregarComentarioSolicitudCommand(long Id, AgregarComentarioRequest Request, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class AgregarComentarioSolicitudCommandHandler(ISolicitudService service)
    : IRequestHandler<AgregarComentarioSolicitudCommand, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        AgregarComentarioSolicitudCommand command,
        CancellationToken cancellationToken) =>
        service.AddCommentAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
