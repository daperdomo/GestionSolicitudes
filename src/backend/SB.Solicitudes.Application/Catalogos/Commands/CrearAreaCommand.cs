using MediatR;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Application.Catalogos;

public sealed record CrearAreaCommand(CrearCatalogoRequest Request) : IRequest<Result<CatalogAdminItem>>;

internal sealed class CrearAreaCommandHandler(ICatalogoAdministrationService service)
    : IRequestHandler<CrearAreaCommand, Result<CatalogAdminItem>>
{
    public Task<Result<CatalogAdminItem>> Handle(CrearAreaCommand command, CancellationToken cancellationToken) =>
        service.CreateAreaAsync(command.Request, cancellationToken);
}
