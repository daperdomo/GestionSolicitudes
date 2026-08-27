using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

public sealed record CrearEntidadGubernamentalCommand(GovernmentEntityRequest Request)
    : IRequest<Result<GovernmentEntity>>;

internal sealed class CrearEntidadGubernamentalCommandHandler(IGovernmentEntityService service)
    : IRequestHandler<CrearEntidadGubernamentalCommand, Result<GovernmentEntity>>
{
    public Task<Result<GovernmentEntity>> Handle(
        CrearEntidadGubernamentalCommand command,
        CancellationToken cancellationToken) =>
        service.CreateAsync(command.Request, cancellationToken);
}
