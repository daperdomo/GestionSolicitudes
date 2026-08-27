using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record ActualizarUsuarioCommand(
    Guid Id,
    ActualizarUsuarioRequest Request,
    CurrentUser CurrentUser) : IRequest<Result<UsuarioListItem>>;

internal sealed class ActualizarUsuarioCommandHandler(IUsuarioAdministrationService service)
    : IRequestHandler<ActualizarUsuarioCommand, Result<UsuarioListItem>>
{
    public Task<Result<UsuarioListItem>> Handle(
        ActualizarUsuarioCommand command,
        CancellationToken cancellationToken) =>
        service.UpdateAsync(command.Id, command.Request, command.CurrentUser, cancellationToken);
}
