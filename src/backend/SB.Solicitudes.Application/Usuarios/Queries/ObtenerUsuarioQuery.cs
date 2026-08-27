using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record ObtenerUsuarioQuery(Guid Id) : IRequest<Result<UsuarioListItem>>;

internal sealed class ObtenerUsuarioQueryHandler(IUsuarioAdministrationService service)
    : IRequestHandler<ObtenerUsuarioQuery, Result<UsuarioListItem>>
{
    public Task<Result<UsuarioListItem>> Handle(
        ObtenerUsuarioQuery query,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(query.Id, cancellationToken);
}
