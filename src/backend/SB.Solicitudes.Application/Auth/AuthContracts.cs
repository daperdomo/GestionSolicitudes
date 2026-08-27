using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Application.Usuarios;

namespace SB.Solicitudes.Application.Auth;

public sealed record LoginRequest(string Correo, string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UsuarioId,
    string Nombre,
    string Correo,
    string Rol);

public sealed record UserOption(Guid Id, string Nombre, string Correo);

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserOption>> GetActiveAnalystsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserOption>> SearchActiveAsync(string? search, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UsuarioListItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludedUserId, CancellationToken cancellationToken);
    Task AddAsync(Usuario user, CancellationToken cancellationToken);
}

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
