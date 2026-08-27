namespace SB.Solicitudes.Domain.Entities;

public sealed class Area
{
    private Area()
    {
    }

    public Area(string nombre)
    {
        Nombre = nombre.Trim();
        Activa = true;
    }

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activa { get; private set; }
}
