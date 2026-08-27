using MediatR;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

public sealed record ObtenerEntidadesGubernamentalesQuery : IRequest<IReadOnlyCollection<GovernmentEntity>>;

internal sealed class ObtenerEntidadesGubernamentalesQueryHandler(IGovernmentEntityService service)
    : IRequestHandler<ObtenerEntidadesGubernamentalesQuery, IReadOnlyCollection<GovernmentEntity>>
{
    public Task<IReadOnlyCollection<GovernmentEntity>> Handle(
        ObtenerEntidadesGubernamentalesQuery query,
        CancellationToken cancellationToken) =>
        service.GetAllAsync(cancellationToken);
}
