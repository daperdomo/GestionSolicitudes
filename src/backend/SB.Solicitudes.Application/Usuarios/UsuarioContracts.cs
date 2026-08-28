using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Application.Usuarios;

public sealed record UsuarioListItem(
    Guid Id,
    string Nombre,
    string Correo,
    RolUsuario Rol,
    bool Activo,
    DateTimeOffset FechaCreacion);

public sealed record CrearUsuarioRequest(
    string Nombre,
    string Correo,
    string Password,
    RolUsuario Rol);

public sealed record ActualizarUsuarioRequest(
    string Nombre,
    string Correo,
    string? Password,
    RolUsuario Rol,
    bool Activo);

public interface IUsuarioAdministrationService
{
    Task<IReadOnlyCollection<UsuarioListItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<UsuarioListItem>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<UsuarioListItem>> CreateAsync(CrearUsuarioRequest request, CancellationToken cancellationToken);
    Task<Result<UsuarioListItem>> UpdateAsync(
        Guid id,
        ActualizarUsuarioRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken);
}
