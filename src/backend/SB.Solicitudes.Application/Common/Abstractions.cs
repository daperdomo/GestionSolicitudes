using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Application.Common;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IPasswordService
{
    string Hash(Usuario usuario, string password);
    bool Verify(Usuario usuario, string passwordHash, string password);
}

public interface ITokenService
{
    TokenResult Create(Usuario usuario);
}

public sealed record TokenResult(string AccessToken, DateTimeOffset ExpiresAt);

public interface ISolicitudCodeGenerator
{
    Task<string> NextAsync(DateTimeOffset currentDate, CancellationToken cancellationToken);
}

public interface INotificationDispatcher
{
    Task DispatchAsync(Notificacion notification, CancellationToken cancellationToken);
}

public interface INotificationRepository
{
    Task AddAsync(Notificacion notification, CancellationToken cancellationToken);
}

public sealed record CurrentUser(Guid Id, RolUsuario Rol);
