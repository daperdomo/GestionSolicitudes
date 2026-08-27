using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Domain.Rules;

namespace SB.Solicitudes.Domain.Entities;

public sealed class Solicitud
{
    private readonly List<Comentario> comentarios = [];
    private readonly List<HistorialAsignacion> historialAsignaciones = [];
    private readonly List<HistorialEstado> historialEstados = [];
    private readonly List<ActividadSolicitud> actividades = [];

    private Solicitud()
    {
    }

    private Solicitud(
        string codigo,
        string titulo,
        string descripcion,
        PrioridadSolicitud prioridad,
        DateTimeOffset? fechaCompromiso,
        Guid usuarioSolicitanteId,
        int areaId,
        int tipoSolicitudId,
        string? evidenciaReferencia,
        Guid? responsableId,
        DateTimeOffset fechaCreacion)
    {
        Codigo = codigo.Trim();
        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        Prioridad = prioridad;
        Estado = EstadoSolicitud.Registrada;
        FechaCreacion = fechaCreacion;
        FechaCompromiso = fechaCompromiso;
        UsuarioSolicitanteId = usuarioSolicitanteId;
        AreaId = areaId;
        TipoSolicitudId = tipoSolicitudId;
        EvidenciaReferencia = evidenciaReferencia?.Trim();
        ResponsableId = responsableId;
        historialEstados.Add(new HistorialEstado(null, Estado, usuarioSolicitanteId, fechaCreacion, "Solicitud registrada."));
        if (responsableId.HasValue)
        {
            historialAsignaciones.Add(new HistorialAsignacion(null, responsableId, usuarioSolicitanteId, fechaCreacion, "Asignación inicial."));
        }
    }

    public long Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public PrioridadSolicitud Prioridad { get; private set; }
    public EstadoSolicitud Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; private set; }
    public DateTimeOffset? FechaCompromiso { get; private set; }
    public Guid UsuarioSolicitanteId { get; private set; }
    public Usuario UsuarioSolicitante { get; private set; } = null!;
    public Guid? ResponsableId { get; private set; }
    public Usuario? Responsable { get; private set; }
    public int AreaId { get; private set; }
    public Area Area { get; private set; } = null!;
    public int TipoSolicitudId { get; private set; }
    public TipoSolicitud TipoSolicitud { get; private set; } = null!;
    public string? EvidenciaReferencia { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<HistorialEstado> HistorialEstados => historialEstados;
    public IReadOnlyCollection<HistorialAsignacion> HistorialAsignaciones => historialAsignaciones;
    public IReadOnlyCollection<Comentario> Comentarios => comentarios;
    public IReadOnlyCollection<ActividadSolicitud> Actividades => actividades;

    public static Solicitud Crear(
        string codigo,
        string titulo,
        string descripcion,
        PrioridadSolicitud prioridad,
        DateTimeOffset? fechaCompromiso,
        Guid usuarioSolicitanteId,
        int areaId,
        int tipoSolicitudId,
        string? evidenciaReferencia,
        DateTimeOffset fechaCreacion)
        => Crear(
            codigo,
            titulo,
            descripcion,
            prioridad,
            fechaCompromiso,
            usuarioSolicitanteId,
            areaId,
            tipoSolicitudId,
            evidenciaReferencia,
            null,
            fechaCreacion);

    public static Solicitud Crear(
        string codigo,
        string titulo,
        string descripcion,
        PrioridadSolicitud prioridad,
        DateTimeOffset? fechaCompromiso,
        Guid usuarioSolicitanteId,
        int areaId,
        int tipoSolicitudId,
        string? evidenciaReferencia,
        Guid? responsableId,
        DateTimeOffset fechaCreacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(titulo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Solicitud(
            codigo,
            titulo,
            descripcion,
            prioridad,
            fechaCompromiso,
            usuarioSolicitanteId,
            areaId,
            tipoSolicitudId,
            evidenciaReferencia,
            responsableId,
            fechaCreacion);
    }

    public RuleResult CambiarEstado(
        EstadoSolicitud nuevoEstado,
        Guid usuarioId,
        RolUsuario rol,
        string? comentario,
        DateTimeOffset fecha)
    {
        if (!EstadoSolicitudTransitions.CanTransition(Estado, nuevoEstado))
        {
            return RuleResult.Failure("invalid_transition", $"No se permite cambiar de {Estado} a {nuevoEstado}.");
        }

        if (nuevoEstado == EstadoSolicitud.Cerrada && string.IsNullOrWhiteSpace(comentario))
        {
            return RuleResult.Failure("resolution_comment_required", "Cerrar una solicitud requiere un comentario de resolución.");
        }

        if (nuevoEstado == EstadoSolicitud.EnEsperaSolicitante && string.IsNullOrWhiteSpace(comentario))
        {
            return RuleResult.Failure("public_comment_required", "Pasar a espera del solicitante requiere un comentario público.");
        }

        if (Estado == EstadoSolicitud.Cerrada && string.IsNullOrWhiteSpace(comentario))
        {
            return RuleResult.Failure("reopen_reason_required", "Reabrir una solicitud requiere indicar el motivo.");
        }

        if (Estado == EstadoSolicitud.Cerrada && rol is not RolUsuario.Administrador and not RolUsuario.Analista)
        {
            return RuleResult.Failure("reopen_forbidden", "Solo un Administrador o Analista puede reabrir una solicitud.");
        }

        EstadoSolicitud estadoAnterior = Estado;
        Estado = nuevoEstado;
        historialEstados.Add(new HistorialEstado(estadoAnterior, nuevoEstado, usuarioId, fecha, comentario));
        if (nuevoEstado == EstadoSolicitud.EnEsperaSolicitante && comentario is not null)
        {
            comentarios.Add(new Comentario(usuarioId, comentario, VisibilidadComentario.Publico, fecha));
        }

        return RuleResult.Success();
    }

    public RuleResult Asignar(Guid? responsableId, Guid usuarioId, string? comentario, DateTimeOffset fecha)
    {
        if (ResponsableId == responsableId)
        {
            return RuleResult.Failure("same_assignee", "La solicitud ya está asignada al responsable indicado.");
        }

        Guid? responsableAnteriorId = ResponsableId;
        ResponsableId = responsableId;
        historialAsignaciones.Add(new HistorialAsignacion(
            responsableAnteriorId,
            responsableId,
            usuarioId,
            fecha,
            comentario));

        return RuleResult.Success();
    }

    public RuleResult CambiarPrioridad(PrioridadSolicitud prioridad, Guid usuarioId, DateTimeOffset fecha)
    {
        if (Prioridad == prioridad)
        {
            return RuleResult.Failure("same_priority", "La solicitud ya tiene la prioridad indicada.");
        }

        PrioridadSolicitud anterior = Prioridad;
        Prioridad = prioridad;
        actividades.Add(new ActividadSolicitud(usuarioId, "Prioridad", anterior.ToString(), prioridad.ToString(), fecha));
        return RuleResult.Success();
    }

    public RuleResult CambiarFechaCompromiso(DateTimeOffset? fechaCompromiso, Guid usuarioId, DateTimeOffset fecha)
    {
        if (FechaCompromiso == fechaCompromiso)
        {
            return RuleResult.Failure("same_due_date", "La solicitud ya tiene la fecha de compromiso indicada.");
        }

        DateTimeOffset? anterior = FechaCompromiso;
        FechaCompromiso = fechaCompromiso;
        actividades.Add(new ActividadSolicitud(usuarioId, "Fecha compromiso", anterior?.ToString("O"), fechaCompromiso?.ToString("O"), fecha));
        return RuleResult.Success();
    }

    public RuleResult CambiarArea(int areaId, string areaAnterior, string areaNueva, Guid usuarioId, DateTimeOffset fecha)
    {
        if (AreaId == areaId)
        {
            return RuleResult.Failure("same_area", "La solicitud ya pertenece al área indicada.");
        }

        AreaId = areaId;
        actividades.Add(new ActividadSolicitud(usuarioId, "Área", areaAnterior, areaNueva, fecha));
        return RuleResult.Success();
    }

    public RuleResult CambiarTipoSolicitud(int tipoSolicitudId, string tipoAnterior, string tipoNuevo, Guid usuarioId, DateTimeOffset fecha)
    {
        if (TipoSolicitudId == tipoSolicitudId)
        {
            return RuleResult.Failure("same_request_type", "La solicitud ya tiene el tipo indicado.");
        }

        TipoSolicitudId = tipoSolicitudId;
        actividades.Add(new ActividadSolicitud(usuarioId, "Tipo", tipoAnterior, tipoNuevo, fecha));
        return RuleResult.Success();
    }

    public RuleResult AgregarComentario(
        Guid usuarioId,
        string texto,
        VisibilidadComentario visibilidad,
        DateTimeOffset fecha)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return RuleResult.Failure("comment_required", "El comentario es obligatorio.");
        }

        comentarios.Add(new Comentario(usuarioId, texto, visibilidad, fecha));
        return RuleResult.Success();
    }
}
