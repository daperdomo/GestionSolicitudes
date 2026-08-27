using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Domain.Entities;

public sealed class Usuario
{
    private Usuario()
    {
    }

    public Usuario(Guid id, string nombre, string correo, RolUsuario rol, bool activo = true)
    {
        Id = id;
        Nombre = nombre.Trim();
        Correo = correo.Trim().ToLowerInvariant();
        Rol = rol;
        Activo = activo;
        FechaCreacion = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }
    public DateTimeOffset FechaCreacion { get; private set; }

    public void EstablecerPasswordHash(string passwordHash)
    {
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ArgumentException("El hash de contraseña es obligatorio.", nameof(passwordHash))
            : passwordHash;
    }

    public void Actualizar(string nombre, string correo, RolUsuario rol, bool activo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(correo);

        Nombre = nombre.Trim();
        Correo = correo.Trim().ToLowerInvariant();
        Rol = rol;
        Activo = activo;
    }
}
