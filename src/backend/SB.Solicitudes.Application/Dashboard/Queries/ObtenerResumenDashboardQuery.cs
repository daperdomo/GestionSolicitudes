using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Dashboard;

public sealed record ObtenerResumenDashboardQuery(CurrentUser CurrentUser) : IRequest<DashboardSummary>;

internal sealed class ObtenerResumenDashboardQueryHandler(IDashboardService service)
    : IRequestHandler<ObtenerResumenDashboardQuery, DashboardSummary>
{
    public Task<DashboardSummary> Handle(
        ObtenerResumenDashboardQuery query,
        CancellationToken cancellationToken) =>
        service.GetSummaryAsync(query.CurrentUser, cancellationToken);
}
