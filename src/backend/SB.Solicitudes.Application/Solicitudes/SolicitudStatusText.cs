using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Application.Solicitudes;

internal static class SolicitudStatusText
{
    public static string ToFriendlyName(this EstadoSolicitud status) => status switch
    {
        EstadoSolicitud.Registrada => "Registrada",
        EstadoSolicitud.EnAnalisis => "En análisis",
        EstadoSolicitud.EnProgreso => "En progreso",
        EstadoSolicitud.EnEsperaSolicitante => "En espera del solicitante",
        EstadoSolicitud.Resuelta => "Resuelta",
        EstadoSolicitud.Cerrada => "Cerrada",
        _ => status.ToString(),
    };
}
