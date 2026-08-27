using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record CrearUsuarioCommand(CrearUsuarioRequest Request) : IRequest<Result<UsuarioListItem>>;

internal sealed class CrearUsuarioCommandHandler(IUsuarioAdministrationService service)
    : IRequestHandler<CrearUsuarioCommand, Result<UsuarioListItem>>
{
    public Task<Result<UsuarioListItem>> Handle(
        CrearUsuarioCommand command,
        CancellationToken cancellationToken) =>
        service.CreateAsync(command.Request, cancellationToken);
}
