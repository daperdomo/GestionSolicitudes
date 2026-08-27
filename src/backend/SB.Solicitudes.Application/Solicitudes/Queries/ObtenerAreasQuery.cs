using MediatR;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record ObtenerAreasQuery : IRequest<IReadOnlyCollection<CatalogItem>>;

internal sealed class ObtenerAreasQueryHandler(ICatalogRepository repository)
    : IRequestHandler<ObtenerAreasQuery, IReadOnlyCollection<CatalogItem>>
{
    public Task<IReadOnlyCollection<CatalogItem>> Handle(
        ObtenerAreasQuery query,
        CancellationToken cancellationToken) =>
        repository.GetAreasAsync(cancellationToken);
}
