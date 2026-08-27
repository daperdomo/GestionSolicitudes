using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Domain.Entities;

public sealed class Notificacion
{
    private Notificacion()
    {
    }

    public Notificacion(
        Solicitud solicitud,
        Guid destinatarioId,
        string asunto,
        string mensaje,
        CanalNotificacion canal,
        DateTimeOffset fecha)
    {
        Solicitud = solicitud;
        DestinatarioId = destinatarioId;
        Asunto = asunto.Trim();
        Mensaje = mensaje.Trim();
        Canal = canal;
        Estado = EstadoNotificacion.Pendiente;
        FechaCreacion = fecha;
    }

    public long Id { get; private set; }
    public long SolicitudId { get; private set; }
    public Solicitud Solicitud { get; private set; } = null!;
    public Guid DestinatarioId { get; private set; }
    public Usuario Destinatario { get; private set; } = null!;
    public string Asunto { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public CanalNotificacion Canal { get; private set; }
    public EstadoNotificacion Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; private set; }
    public DateTimeOffset? FechaEnvio { get; private set; }

    public void MarcarComoEnviada(DateTimeOffset fecha)
    {
        Estado = EstadoNotificacion.Enviada;
        FechaEnvio = fecha;
    }
}
