using MediatR;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Usuarios;

namespace SB.Solicitudes.Application.Auth;

public sealed record RegistrarSolicitanteCommand(RegistrarSolicitanteRequest Request)
    : IRequest<Result<UsuarioRegistradoResponse>>;

internal sealed class RegistrarSolicitanteCommandHandler(IUsuarioRegistrationService service)
    : IRequestHandler<RegistrarSolicitanteCommand, Result<UsuarioRegistradoResponse>>
{
    public Task<Result<UsuarioRegistradoResponse>> Handle(
        RegistrarSolicitanteCommand command,
        CancellationToken cancellationToken) =>
        service.RegisterRequesterAsync(command.Request, cancellationToken);
}
