using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Dashboard;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Infrastructure.Persistence;

namespace SB.Solicitudes.Infrastructure.Repositories;

internal sealed class DashboardRepository(ApplicationDbContext dbContext) : IDashboardRepository
{
    public async Task<DashboardSummary> GetSummaryAsync(
        CurrentUser currentUser,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        IQueryable<Solicitud> query = dbContext.Solicitudes.AsNoTracking();
        if (currentUser.Rol == RolUsuario.Solicitante)
        {
            query = query.Where(request => request.UsuarioSolicitanteId == currentUser.Id);
        }

        int open = await query.CountAsync(request => request.Estado != EstadoSolicitud.Cerrada, cancellationToken);
        int closed = await query.CountAsync(request => request.Estado == EstadoSolicitud.Cerrada, cancellationToken);
        int overdue = await query.CountAsync(
            request => request.FechaCompromiso < now && request.Estado != EstadoSolicitud.Cerrada,
            cancellationToken);
        var priorityGroups = await query
            .GroupBy(request => request.Prioridad)
            .Select(group => new { Nombre = group.Key, Total = group.Count() })
            .ToListAsync(cancellationToken);
        var statusGroups = await query
            .GroupBy(request => request.Estado)
            .Select(group => new { Nombre = group.Key, Total = group.Count() })
            .ToListAsync(cancellationToken);
        List<MetricItem> byPriority = priorityGroups
            .Select(group => new MetricItem(group.Nombre.ToString(), group.Total))
            .ToList();
        List<MetricItem> byStatus = statusGroups
            .Select(group => new MetricItem(group.Nombre.ToString(), group.Total))
            .ToList();
        List<SolicitudListItem> latest = await query
            .OrderByDescending(request => request.FechaCreacion)
            .Take(5)
            .Select(request => new SolicitudListItem(
                request.Id,
                request.Codigo,
                request.Titulo,
                request.Prioridad,
                request.Estado,
                request.FechaCreacion,
                request.FechaCompromiso,
                request.Area.Nombre,
                request.TipoSolicitud.Nombre,
                request.UsuarioSolicitante.Nombre,
                request.Responsable == null ? null : request.Responsable.Nombre))
            .ToListAsync(cancellationToken);

        return new DashboardSummary(open, closed, overdue, byPriority, byStatus, latest);
    }
}
