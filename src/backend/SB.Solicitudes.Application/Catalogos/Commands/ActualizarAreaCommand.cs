using MediatR;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Application.Catalogos;

public sealed record ActualizarAreaCommand(int Id, ActualizarCatalogoRequest Request) : IRequest<Result<CatalogAdminItem>>;

internal sealed class ActualizarAreaCommandHandler(ICatalogoAdministrationService service)
    : IRequestHandler<ActualizarAreaCommand, Result<CatalogAdminItem>>
{
    public Task<Result<CatalogAdminItem>> Handle(ActualizarAreaCommand command, CancellationToken cancellationToken) =>
        service.UpdateAreaAsync(command.Id, command.Request, cancellationToken);
}
