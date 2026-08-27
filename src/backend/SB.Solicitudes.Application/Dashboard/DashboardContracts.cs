using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Application.Dashboard;

public sealed record MetricItem(string Nombre, int Total);

public sealed record DashboardSummary(
    int SolicitudesAbiertas,
    int SolicitudesCerradas,
    int SolicitudesVencidas,
    IReadOnlyCollection<MetricItem> PorPrioridad,
    IReadOnlyCollection<MetricItem> PorEstado,
    IReadOnlyCollection<SolicitudListItem> UltimasSolicitudes);

public interface IDashboardRepository
{
    Task<DashboardSummary> GetSummaryAsync(
        CurrentUser currentUser,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(CurrentUser currentUser, CancellationToken cancellationToken);
}
