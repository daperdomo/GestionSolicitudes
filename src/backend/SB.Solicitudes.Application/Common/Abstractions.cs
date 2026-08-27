using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Application.Notifications;

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
    Task DispatchAsync(NotificationDispatchMessage message, CancellationToken cancellationToken);
}

public sealed record NotificationDispatchMessage(
    long NotificationId,
    long SolicitudId,
    string CodigoSolicitud,
    Guid RecipientId,
    string Subject,
    string Message,
    DateTimeOffset CreatedAt);

public interface INotificationRepository
{
    Task AddAsync(Notificacion notification, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificationItem>> GetLatestAsync(
        Guid recipientId,
        int limit,
        CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken);
    Task<Notificacion?> GetForRecipientAsync(long id, Guid recipientId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Notificacion>> GetUnreadForRecipientAsync(
        Guid recipientId,
        CancellationToken cancellationToken);
}

public sealed record CurrentUser(Guid Id, RolUsuario Rol);
