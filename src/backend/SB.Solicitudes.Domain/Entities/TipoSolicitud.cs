namespace SB.Solicitudes.Domain.Entities;

public sealed class TipoSolicitud
{
    private TipoSolicitud()
    {
    }

    public TipoSolicitud(string nombre)
    {
        Nombre = nombre.Trim();
        Activo = true;
    }

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; }
}
