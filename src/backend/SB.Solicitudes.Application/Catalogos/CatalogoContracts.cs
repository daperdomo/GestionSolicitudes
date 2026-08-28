using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Application.Catalogos;

public sealed record CrearCatalogoRequest(string Nombre);
public sealed record ActualizarCatalogoRequest(string Nombre, bool Activo);
public sealed record CatalogosAdministracion(
    IReadOnlyCollection<CatalogAdminItem> Areas,
    IReadOnlyCollection<CatalogAdminItem> TiposSolicitud);

public interface ICatalogoAdministrationService
{
    Task<CatalogosAdministracion> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<CatalogAdminItem>> CreateAreaAsync(CrearCatalogoRequest request, CancellationToken cancellationToken);
    Task<Result<CatalogAdminItem>> UpdateAreaAsync(int id, ActualizarCatalogoRequest request, CancellationToken cancellationToken);
    Task<Result<CatalogAdminItem>> CreateRequestTypeAsync(CrearCatalogoRequest request, CancellationToken cancellationToken);
    Task<Result<CatalogAdminItem>> UpdateRequestTypeAsync(int id, ActualizarCatalogoRequest request, CancellationToken cancellationToken);
}
