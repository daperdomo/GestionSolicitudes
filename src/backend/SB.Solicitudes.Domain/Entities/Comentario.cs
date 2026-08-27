using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Domain.Entities;

public sealed class Comentario
{
    private Comentario()
    {
    }

    internal Comentario(Guid usuarioId, string texto, VisibilidadComentario visibilidad, DateTimeOffset fecha)
    {
        UsuarioId = usuarioId;
        Texto = texto.Trim();
        Visibilidad = visibilidad;
        Fecha = fecha;
    }

    public long Id { get; private set; }
    public long SolicitudId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Texto { get; private set; } = string.Empty;
    public VisibilidadComentario Visibilidad { get; private set; }
    public DateTimeOffset Fecha { get; private set; }
}
