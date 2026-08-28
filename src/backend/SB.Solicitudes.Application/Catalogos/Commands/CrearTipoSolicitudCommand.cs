using MediatR;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Application.Catalogos;

public sealed record CrearTipoSolicitudCommand(CrearCatalogoRequest Request) : IRequest<Result<CatalogAdminItem>>;

internal sealed class CrearTipoSolicitudCommandHandler(ICatalogoAdministrationService service)
    : IRequestHandler<CrearTipoSolicitudCommand, Result<CatalogAdminItem>>
{
    public Task<Result<CatalogAdminItem>> Handle(CrearTipoSolicitudCommand command, CancellationToken cancellationToken) =>
        service.CreateRequestTypeAsync(command.Request, cancellationToken);
}
