namespace SB.Solicitudes.Domain.Entities;

public sealed class ActividadSolicitud
{
    private ActividadSolicitud()
    {
    }

    internal ActividadSolicitud(
        Guid usuarioId,
        string campo,
        string? valorAnterior,
        string? valorNuevo,
        DateTimeOffset fecha)
    {
        UsuarioId = usuarioId;
        Campo = campo;
        ValorAnterior = valorAnterior;
        ValorNuevo = valorNuevo;
        Fecha = fecha;
    }

    public long Id { get; private set; }
    public long SolicitudId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Campo { get; private set; } = string.Empty;
    public string? ValorAnterior { get; private set; }
    public string? ValorNuevo { get; private set; }
    public DateTimeOffset Fecha { get; private set; }
}
