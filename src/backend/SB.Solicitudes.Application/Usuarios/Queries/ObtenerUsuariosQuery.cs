using MediatR;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record ObtenerUsuariosQuery : IRequest<IReadOnlyCollection<UsuarioListItem>>;

internal sealed class ObtenerUsuariosQueryHandler(IUsuarioAdministrationService service)
    : IRequestHandler<ObtenerUsuariosQuery, IReadOnlyCollection<UsuarioListItem>>
{
    public Task<IReadOnlyCollection<UsuarioListItem>> Handle(
        ObtenerUsuariosQuery query,
        CancellationToken cancellationToken) =>
        service.GetAllAsync(cancellationToken);
}
