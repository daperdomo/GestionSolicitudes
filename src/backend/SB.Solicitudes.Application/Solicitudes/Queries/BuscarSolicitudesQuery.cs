using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record BuscarSolicitudesQuery(SolicitudFilter Filter, CurrentUser CurrentUser)
    : IRequest<Result<PagedResult<SolicitudListItem>>>;

internal sealed class BuscarSolicitudesQueryHandler(ISolicitudService service)
    : IRequestHandler<BuscarSolicitudesQuery, Result<PagedResult<SolicitudListItem>>>
{
    public Task<Result<PagedResult<SolicitudListItem>>> Handle(
        BuscarSolicitudesQuery query,
        CancellationToken cancellationToken) =>
        service.SearchAsync(query.Filter, query.CurrentUser, cancellationToken);
}
