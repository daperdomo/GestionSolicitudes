using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Dashboard;

internal sealed class DashboardService(IDashboardRepository repository) : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(
        CurrentUser currentUser,
        CancellationToken cancellationToken) =>
        await repository.GetSummaryAsync(currentUser, DateTimeOffset.UtcNow, cancellationToken);
}
