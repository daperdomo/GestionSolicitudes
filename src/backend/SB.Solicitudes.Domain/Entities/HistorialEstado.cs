using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Domain.Entities;

public sealed class HistorialEstado
{
    private HistorialEstado()
    {
    }

    internal HistorialEstado(
        EstadoSolicitud? estadoAnterior,
        EstadoSolicitud estadoNuevo,
        Guid usuarioId,
        DateTimeOffset fecha,
        string? comentario)
    {
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        UsuarioId = usuarioId;
        Fecha = fecha;
        Comentario = comentario?.Trim();
    }

    public long Id { get; private set; }
    public long SolicitudId { get; private set; }
    public EstadoSolicitud? EstadoAnterior { get; private set; }
    public EstadoSolicitud EstadoNuevo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public DateTimeOffset Fecha { get; private set; }
    public string? Comentario { get; private set; }
}
