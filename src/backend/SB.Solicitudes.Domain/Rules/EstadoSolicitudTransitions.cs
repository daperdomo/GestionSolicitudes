using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Domain.Rules;

public static class EstadoSolicitudTransitions
{
    public static bool CanTransition(EstadoSolicitud current, EstadoSolicitud next) => current switch
    {
        EstadoSolicitud.Registrada => next is EstadoSolicitud.EnAnalisis,
        EstadoSolicitud.EnAnalisis => next is EstadoSolicitud.EnProgreso
            or EstadoSolicitud.EnEsperaSolicitante,
        EstadoSolicitud.EnProgreso => next is EstadoSolicitud.EnEsperaSolicitante
            or EstadoSolicitud.Resuelta,
        EstadoSolicitud.EnEsperaSolicitante => next is EstadoSolicitud.EnAnalisis
            or EstadoSolicitud.EnProgreso,
        EstadoSolicitud.Resuelta => next is EstadoSolicitud.EnProgreso
            or EstadoSolicitud.Cerrada,
        EstadoSolicitud.Cerrada => next is EstadoSolicitud.EnAnalisis,
        _ => false,
    };
}
