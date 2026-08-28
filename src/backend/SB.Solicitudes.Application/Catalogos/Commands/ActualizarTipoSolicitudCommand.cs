using MediatR;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Application.Catalogos;

public sealed record ActualizarTipoSolicitudCommand(int Id, ActualizarCatalogoRequest Request) : IRequest<Result<CatalogAdminItem>>;

internal sealed class ActualizarTipoSolicitudCommandHandler(ICatalogoAdministrationService service)
    : IRequestHandler<ActualizarTipoSolicitudCommand, Result<CatalogAdminItem>>
{
    public Task<Result<CatalogAdminItem>> Handle(ActualizarTipoSolicitudCommand command, CancellationToken cancellationToken) =>
        service.UpdateRequestTypeAsync(command.Id, command.Request, cancellationToken);
}
