using MediatR;
using SB.Solicitudes.Application.Auth;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record BuscarUsuariosAsignablesQuery(string? Search) : IRequest<IReadOnlyCollection<UserOption>>;

internal sealed class BuscarUsuariosAsignablesQueryHandler(IUsuarioRepository repository)
    : IRequestHandler<BuscarUsuariosAsignablesQuery, IReadOnlyCollection<UserOption>>
{
    public Task<IReadOnlyCollection<UserOption>> Handle(
        BuscarUsuariosAsignablesQuery query,
        CancellationToken cancellationToken) =>
        repository.SearchActiveAsync(query.Search, cancellationToken);
}
