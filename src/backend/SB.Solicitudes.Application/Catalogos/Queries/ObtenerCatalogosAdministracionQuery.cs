using MediatR;

namespace SB.Solicitudes.Application.Catalogos;

public sealed record ObtenerCatalogosAdministracionQuery : IRequest<CatalogosAdministracion>;

internal sealed class ObtenerCatalogosAdministracionQueryHandler(ICatalogoAdministrationService service)
    : IRequestHandler<ObtenerCatalogosAdministracionQuery, CatalogosAdministracion>
{
    public Task<CatalogosAdministracion> Handle(
        ObtenerCatalogosAdministracionQuery query,
        CancellationToken cancellationToken) =>
        service.GetAllAsync(cancellationToken);
}
