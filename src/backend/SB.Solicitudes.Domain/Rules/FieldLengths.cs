namespace SB.Solicitudes.Domain.Rules;

public static class FieldLengths
{
    public static int UsuarioNombre => 150;
    public static int UsuarioCorreo => 254;
    public static int CatalogoNombre => 120;
    public static int SolicitudCodigo => 24;
    public static int SolicitudTitulo => 200;
    public static int SolicitudDescripcion => 4_000;
    public static int EvidenciaReferencia => 1_000;
    public static int ComentarioTexto => 2_000;
    public static int HistorialComentario => 2_000;
    public static int NotificacionAsunto => 200;
    public static int NotificacionMensaje => 2_000;
    public static int ActividadCampo => 80;
    public static int ActividadValor => 500;
}
