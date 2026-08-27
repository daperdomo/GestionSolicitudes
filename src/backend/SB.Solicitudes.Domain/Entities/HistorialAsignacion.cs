namespace SB.Solicitudes.Domain.Entities;

public sealed class HistorialAsignacion
{
    private HistorialAsignacion()
    {
    }

    internal HistorialAsignacion(
        Guid? responsableAnteriorId,
        Guid? responsableNuevoId,
        Guid usuarioId,
        DateTimeOffset fecha,
        string? comentario)
    {
        ResponsableAnteriorId = responsableAnteriorId;
        ResponsableNuevoId = responsableNuevoId;
        UsuarioId = usuarioId;
        Fecha = fecha;
        Comentario = comentario?.Trim();
    }

    public long Id { get; private set; }
    public long SolicitudId { get; private set; }
    public Guid? ResponsableAnteriorId { get; private set; }
    public Usuario? ResponsableAnterior { get; private set; }
    public Guid? ResponsableNuevoId { get; private set; }
    public Usuario? ResponsableNuevo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public DateTimeOffset Fecha { get; private set; }
    public string? Comentario { get; private set; }
}
