using MediatR;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record ObtenerTiposSolicitudQuery : IRequest<IReadOnlyCollection<CatalogItem>>;

internal sealed class ObtenerTiposSolicitudQueryHandler(ICatalogRepository repository)
    : IRequestHandler<ObtenerTiposSolicitudQuery, IReadOnlyCollection<CatalogItem>>
{
    public Task<IReadOnlyCollection<CatalogItem>> Handle(
        ObtenerTiposSolicitudQuery query,
        CancellationToken cancellationToken) =>
        repository.GetTiposSolicitudAsync(cancellationToken);
}
