using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

public sealed record ObtenerEntidadGubernamentalQuery(int Id) : IRequest<Result<GovernmentEntity>>;

internal sealed class ObtenerEntidadGubernamentalQueryHandler(IGovernmentEntityService service)
    : IRequestHandler<ObtenerEntidadGubernamentalQuery, Result<GovernmentEntity>>
{
    public Task<Result<GovernmentEntity>> Handle(
        ObtenerEntidadGubernamentalQuery query,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(query.Id, cancellationToken);
}
