using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

public sealed record ActualizarEntidadGubernamentalCommand(int Id, GovernmentEntityRequest Request)
    : IRequest<Result<GovernmentEntity>>;

internal sealed class ActualizarEntidadGubernamentalCommandHandler(IGovernmentEntityService service)
    : IRequestHandler<ActualizarEntidadGubernamentalCommand, Result<GovernmentEntity>>
{
    public Task<Result<GovernmentEntity>> Handle(
        ActualizarEntidadGubernamentalCommand command,
        CancellationToken cancellationToken) =>
        service.UpdateAsync(command.Id, command.Request, cancellationToken);
}
