using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

public sealed record EliminarEntidadGubernamentalCommand(int Id) : IRequest<Result<bool>>;

internal sealed class EliminarEntidadGubernamentalCommandHandler(IGovernmentEntityService service)
    : IRequestHandler<EliminarEntidadGubernamentalCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(
        EliminarEntidadGubernamentalCommand command,
        CancellationToken cancellationToken) =>
        service.DeleteAsync(command.Id, cancellationToken);
}
