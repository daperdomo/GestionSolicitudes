using MediatR;
using SB.Solicitudes.Application.Auth;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record ObtenerAnalistasActivosQuery : IRequest<IReadOnlyCollection<UserOption>>;

internal sealed class ObtenerAnalistasActivosQueryHandler(IUsuarioRepository repository)
    : IRequestHandler<ObtenerAnalistasActivosQuery, IReadOnlyCollection<UserOption>>
{
    public Task<IReadOnlyCollection<UserOption>> Handle(
        ObtenerAnalistasActivosQuery query,
        CancellationToken cancellationToken) =>
        repository.GetActiveAnalystsAsync(cancellationToken);
}
