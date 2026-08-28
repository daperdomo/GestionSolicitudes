using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Application.Solicitudes;

public sealed record CrearSolicitudRequest(
    string Titulo,
    string Descripcion,
    PrioridadSolicitud Prioridad,
    DateTimeOffset? FechaCompromiso,
    int AreaId,
    int TipoSolicitudId,
    string? EvidenciaReferencia,
    Guid? ResponsableId);

public sealed record CambiarEstadoRequest(EstadoSolicitud Estado, string? Comentario, string RowVersion);
public sealed record AsignarSolicitudRequest(Guid? ResponsableId, string? Comentario);
public sealed record AgregarComentarioRequest(string Texto, VisibilidadComentario Visibilidad);
public sealed record CambiarPrioridadRequest(PrioridadSolicitud Prioridad, string RowVersion);
public sealed record CambiarFechaCompromisoRequest(DateTimeOffset? FechaCompromiso, string RowVersion);
public sealed record CambiarAreaRequest(int AreaId, string RowVersion);
public sealed record CambiarTipoSolicitudRequest(int TipoSolicitudId, string RowVersion);

public sealed class SolicitudFilter
{
    public EstadoSolicitud? Estado { get; init; }
    public PrioridadSolicitud? Prioridad { get; init; }
    public int? AreaId { get; init; }
    public int? TipoSolicitudId { get; init; }
    public Guid? SolicitanteId { get; init; }
    public Guid? ResponsableId { get; init; }
    public bool? SinAsignar { get; init; }
    public DateTimeOffset? FechaDesde { get; init; }
    public DateTimeOffset? FechaHasta { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "fechaCreacion";
    public bool Descending { get; init; } = true;
}

public sealed record SolicitudListItem(
    long Id,
    string Codigo,
    string Titulo,
    PrioridadSolicitud Prioridad,
    EstadoSolicitud Estado,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaCompromiso,
    string Area,
    string TipoSolicitud,
    string Solicitante,
    string? Responsable);

public sealed record HistorialEstadoDto(
    EstadoSolicitud? EstadoAnterior,
    EstadoSolicitud EstadoNuevo,
    string Usuario,
    DateTimeOffset Fecha,
    string? Comentario);

public sealed record ComentarioDto(
    long Id,
    string Usuario,
    string Texto,
    VisibilidadComentario Visibilidad,
    DateTimeOffset Fecha);

public sealed record ActividadSolicitudDto(
    string Tipo,
    string Usuario,
    DateTimeOffset Fecha,
    string Descripcion,
    string? ValorAnterior,
    string? ValorNuevo);

public sealed record SolicitudDetail(
    long Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    PrioridadSolicitud Prioridad,
    EstadoSolicitud Estado,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaCompromiso,
    Guid UsuarioSolicitanteId,
    string Solicitante,
    Guid? ResponsableId,
    string? Responsable,
    int AreaId,
    string Area,
    int TipoSolicitudId,
    string TipoSolicitud,
    string? EvidenciaReferencia,
    byte[] RowVersion,
    IReadOnlyCollection<HistorialEstadoDto> Historial,
    IReadOnlyCollection<ComentarioDto> Comentarios,
    IReadOnlyCollection<ActividadSolicitudDto> Actividad);

public interface ISolicitudRepository
{
    Task AddAsync(Solicitud solicitud, CancellationToken cancellationToken);
    Task<Solicitud?> GetForUpdateAsync(long id, CancellationToken cancellationToken);
    void SetOriginalRowVersion(Solicitud solicitud, byte[] rowVersion);
    Task<SolicitudDetail?> GetDetailAsync(long id, bool includeInternalComments, CancellationToken cancellationToken);
    Task<PagedResult<SolicitudListItem>> SearchAsync(
        SolicitudFilter filter,
        CurrentUser currentUser,
        CancellationToken cancellationToken);
}

public interface ICatalogRepository
{
    Task<bool> AreaExistsAsync(int id, CancellationToken cancellationToken);
    Task<bool> TipoSolicitudExistsAsync(int id, CancellationToken cancellationToken);
    Task<CatalogItem?> GetAreaByIdAsync(int id, CancellationToken cancellationToken);
    Task<CatalogItem?> GetTipoSolicitudByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogItem>> GetAreasAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogItem>> GetTiposSolicitudAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogAdminItem>> GetAllAreasAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogAdminItem>> GetAllTiposSolicitudAsync(CancellationToken cancellationToken);
    Task<Area?> GetAreaForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<TipoSolicitud?> GetTipoSolicitudForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<bool> AreaNameExistsAsync(string nombre, int? excludedId, CancellationToken cancellationToken);
    Task<bool> TipoSolicitudNameExistsAsync(string nombre, int? excludedId, CancellationToken cancellationToken);
    Task AddAreaAsync(Area area, CancellationToken cancellationToken);
    Task AddTipoSolicitudAsync(TipoSolicitud tipoSolicitud, CancellationToken cancellationToken);
}

public sealed record CatalogItem(int Id, string Nombre);
public sealed record CatalogAdminItem(int Id, string Nombre, bool Activo);

public interface ISolicitudService
{
    Task<Result<SolicitudDetail>> CreateAsync(
        CrearSolicitudRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    Task<Result<PagedResult<SolicitudListItem>>> SearchAsync(
        SolicitudFilter filter,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    Task<Result<SolicitudDetail>> GetByIdAsync(
        long id,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    Task<Result<SolicitudDetail>> ChangeStatusAsync(
        long id,
        CambiarEstadoRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    Task<Result<SolicitudDetail>> AssignAsync(
        long id,
        AsignarSolicitudRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    Task<Result<SolicitudDetail>> AddCommentAsync(
        long id,
        AgregarComentarioRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    Task<Result<SolicitudDetail>> ChangePriorityAsync(long id, CambiarPrioridadRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    Task<Result<SolicitudDetail>> ChangeDueDateAsync(long id, CambiarFechaCompromisoRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    Task<Result<SolicitudDetail>> ChangeAreaAsync(long id, CambiarAreaRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    Task<Result<SolicitudDetail>> ChangeRequestTypeAsync(long id, CambiarTipoSolicitudRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
}
