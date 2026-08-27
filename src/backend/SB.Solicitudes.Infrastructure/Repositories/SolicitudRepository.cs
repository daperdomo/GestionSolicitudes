using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Infrastructure.Persistence;

namespace SB.Solicitudes.Infrastructure.Repositories;

internal sealed class SolicitudRepository(ApplicationDbContext dbContext) : ISolicitudRepository
{
    public async Task AddAsync(Solicitud solicitud, CancellationToken cancellationToken) =>
        await dbContext.Solicitudes.AddAsync(solicitud, cancellationToken);

    public async Task<Solicitud?> GetForUpdateAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.Solicitudes
            .Include(request => request.Area)
            .Include(request => request.TipoSolicitud)
            .Include(request => request.Responsable)
            .SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    public void SetOriginalRowVersion(Solicitud solicitud, byte[] rowVersion) =>
        dbContext.Entry(solicitud).Property(request => request.RowVersion).OriginalValue = rowVersion;

    public async Task<SolicitudDetail?> GetDetailAsync(
        long id,
        bool includeInternalComments,
        CancellationToken cancellationToken) =>
        await GetDetailCoreAsync(id, includeInternalComments, cancellationToken);

    private async Task<SolicitudDetail?> GetDetailCoreAsync(
        long id,
        bool includeInternalComments,
        CancellationToken cancellationToken)
    {
        SolicitudDetail? detail = await dbContext.Solicitudes
            .AsNoTracking()
            .AsSplitQuery()
            .Where(request => request.Id == id)
            .Select(request => new SolicitudDetail(
                request.Id,
                request.Codigo,
                request.Titulo,
                request.Descripcion,
                request.Prioridad,
                request.Estado,
                request.FechaCreacion,
                request.FechaCompromiso,
                request.UsuarioSolicitanteId,
                request.UsuarioSolicitante.Nombre,
                request.ResponsableId,
                request.Responsable == null ? null : request.Responsable.Nombre,
                request.AreaId,
                request.Area.Nombre,
                request.TipoSolicitudId,
                request.TipoSolicitud.Nombre,
                request.EvidenciaReferencia,
                request.RowVersion,
                request.HistorialEstados
                    .OrderByDescending(history => history.Fecha)
                    .Select(history => new HistorialEstadoDto(
                        history.EstadoAnterior,
                        history.EstadoNuevo,
                        history.Usuario.Nombre,
                        history.Fecha,
                        history.Comentario))
                    .ToList(),
                request.Comentarios
                    .Where(comment => includeInternalComments || comment.Visibilidad == VisibilidadComentario.Publico)
                    .OrderByDescending(comment => comment.Fecha)
                    .Select(comment => new ComentarioDto(
                        comment.Id,
                        comment.Usuario.Nombre,
                        comment.Texto,
                        comment.Visibilidad,
                        comment.Fecha))
                    .ToList(),
                new List<ActividadSolicitudDto>()))
            .SingleOrDefaultAsync(cancellationToken);

        if (detail is null)
        {
            return null;
        }

        List<ActividadSolicitudDto> fieldActivities = await dbContext.ActividadesSolicitud.AsNoTracking()
            .Where(activity => activity.SolicitudId == id)
            .Select(activity => new ActividadSolicitudDto(
                "CampoModificado",
                activity.Usuario.Nombre,
                activity.Fecha,
                $"Cambió {activity.Campo}.",
                activity.ValorAnterior,
                activity.ValorNuevo))
            .ToListAsync(cancellationToken);

        List<ActividadSolicitudDto> stateActivities = await dbContext.HistorialEstados.AsNoTracking()
            .Where(history => history.SolicitudId == id)
            .Select(history => new ActividadSolicitudDto(
                history.EstadoAnterior == null ? "Creacion" : "CambioEstado",
                history.Usuario.Nombre,
                history.Fecha,
                history.EstadoAnterior == null ? "Creó la solicitud." : "Cambió el estado.",
                history.EstadoAnterior == null ? null : history.EstadoAnterior.ToString(),
                history.EstadoNuevo.ToString()))
            .ToListAsync(cancellationToken);

        List<ActividadSolicitudDto> assignmentActivities = await dbContext.HistorialAsignaciones.AsNoTracking()
            .Where(history => history.SolicitudId == id)
            .Select(history => new ActividadSolicitudDto(
                history.ResponsableAnteriorId == null ? "Asignacion" : "Reasignacion",
                history.Usuario.Nombre,
                history.Fecha,
                "Cambió el responsable.",
                history.ResponsableAnterior == null ? "Sin asignar" : history.ResponsableAnterior.Nombre,
                history.ResponsableNuevo == null ? "Sin asignar" : history.ResponsableNuevo.Nombre))
            .ToListAsync(cancellationToken);

        List<ActividadSolicitudDto> commentActivities = await dbContext.Comentarios.AsNoTracking()
            .Where(comment => comment.SolicitudId == id
                && (includeInternalComments || comment.Visibilidad == VisibilidadComentario.Publico))
            .Select(comment => new ActividadSolicitudDto(
                "Comentario",
                comment.Usuario.Nombre,
                comment.Fecha,
                comment.Visibilidad == VisibilidadComentario.Interno
                    ? "Agregó un comentario interno."
                    : "Agregó un comentario público.",
                null,
                null))
            .ToListAsync(cancellationToken);

        IReadOnlyCollection<ActividadSolicitudDto> activity = fieldActivities
            .Concat(stateActivities)
            .Concat(assignmentActivities)
            .Concat(commentActivities)
            .OrderByDescending(item => item.Fecha)
            .ToList();

        return detail with { Actividad = activity };
    }

    public async Task<PagedResult<SolicitudListItem>> SearchAsync(
        SolicitudFilter filter,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        IQueryable<Solicitud> query = dbContext.Solicitudes.AsNoTracking();

        if (currentUser.Rol == RolUsuario.Solicitante)
        {
            query = query.Where(request => request.UsuarioSolicitanteId == currentUser.Id);
        }

        query = ApplyFilters(query, filter);
        int totalItems = await query.CountAsync(cancellationToken);
        query = ApplyOrdering(query, filter);

        List<SolicitudListItem> items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
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

        return new PagedResult<SolicitudListItem>(items, filter.PageNumber, filter.PageSize, totalItems);
    }

    private static IQueryable<Solicitud> ApplyFilters(IQueryable<Solicitud> query, SolicitudFilter filter)
    {
        if (filter.Estado.HasValue)
        {
            query = query.Where(request => request.Estado == filter.Estado.Value);
        }

        if (filter.Prioridad.HasValue)
        {
            query = query.Where(request => request.Prioridad == filter.Prioridad.Value);
        }

        if (filter.AreaId.HasValue)
        {
            query = query.Where(request => request.AreaId == filter.AreaId.Value);
        }

        if (filter.TipoSolicitudId.HasValue)
        {
            query = query.Where(request => request.TipoSolicitudId == filter.TipoSolicitudId.Value);
        }

        if (filter.SolicitanteId.HasValue)
        {
            query = query.Where(request => request.UsuarioSolicitanteId == filter.SolicitanteId.Value);
        }

        if (filter.ResponsableId.HasValue)
        {
            query = query.Where(request => request.ResponsableId == filter.ResponsableId.Value);
        }

        if (filter.FechaDesde.HasValue)
        {
            query = query.Where(request => request.FechaCreacion >= filter.FechaDesde.Value);
        }

        if (filter.FechaHasta.HasValue)
        {
            query = query.Where(request => request.FechaCreacion <= filter.FechaHasta.Value);
        }

        return query;
    }

    private static IQueryable<Solicitud> ApplyOrdering(IQueryable<Solicitud> query, SolicitudFilter filter) =>
        (filter.SortBy.ToLowerInvariant(), filter.Descending) switch
        {
            ("codigo", false) => query.OrderBy(request => request.Codigo),
            ("codigo", true) => query.OrderByDescending(request => request.Codigo),
            ("prioridad", false) => query.OrderBy(request => request.Prioridad),
            ("prioridad", true) => query.OrderByDescending(request => request.Prioridad),
            ("estado", false) => query.OrderBy(request => request.Estado),
            ("estado", true) => query.OrderByDescending(request => request.Estado),
            ("fechacompromiso", false) => query.OrderBy(request => request.FechaCompromiso),
            ("fechacompromiso", true) => query.OrderByDescending(request => request.FechaCompromiso),
            (_, false) => query.OrderBy(request => request.FechaCreacion),
            _ => query.OrderByDescending(request => request.FechaCreacion),
        };
}
