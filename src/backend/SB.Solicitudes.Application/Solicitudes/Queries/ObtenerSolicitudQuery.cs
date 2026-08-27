using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record ObtenerSolicitudQuery(long Id, CurrentUser CurrentUser)
    : IRequest<Result<SolicitudDetail>>;

internal sealed class ObtenerSolicitudQueryHandler(ISolicitudService service)
    : IRequestHandler<ObtenerSolicitudQuery, Result<SolicitudDetail>>
{
    public Task<Result<SolicitudDetail>> Handle(
        ObtenerSolicitudQuery query,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(query.Id, query.CurrentUser, cancellationToken);
}
