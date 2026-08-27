using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    ApplicationDbContext dbContext,
    IPasswordService passwords,
    ISolicitudCodeGenerator codeGenerator,
    ILogger<DatabaseInitializer> logger)
{
    private static readonly Action<ILogger, Exception?> LogSeedVerified = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2001, "SeedVerified"),
        "Datos iniciales de SB.Solicitudes verificados");

    private static readonly Guid ADMINISTRATOR_ID = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ANALYST_ID = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid REQUESTER_ID = Guid.Parse("30000000-0000-0000-0000-000000000003");

    public async Task InitialiseAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await SeedAsync(cancellationToken);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!await dbContext.Areas.AnyAsync(cancellationToken))
        {
            dbContext.Areas.AddRange(
                new Area("Tecnología"),
                new Area("Operaciones"),
                new Area("Seguridad de la Información"));
        }

        if (!await dbContext.TiposSolicitud.AnyAsync(cancellationToken))
        {
            dbContext.TiposSolicitud.AddRange(
                new TipoSolicitud("Soporte técnico"),
                new TipoSolicitud("Acceso a sistemas"),
                new TipoSolicitud("Desarrollo o mejora"));
        }

        if (!await dbContext.Usuarios.AnyAsync(cancellationToken))
        {
            Usuario administrator = CreateUser(ADMINISTRATOR_ID, "Administrador SB", "admin@sb.local", RolUsuario.Administrador, "Admin1234!");
            Usuario analyst = CreateUser(ANALYST_ID, "Analista SB", "analista@sb.local", RolUsuario.Analista, "Analista1234!");
            Usuario requester = CreateUser(REQUESTER_ID, "Solicitante SB", "solicitante@sb.local", RolUsuario.Solicitante, "Solicita1234!");
            dbContext.Usuarios.AddRange(administrator, analyst, requester);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!await dbContext.Solicitudes.AnyAsync(cancellationToken))
        {
            int areaId = await dbContext.Areas.OrderBy(area => area.Id).Select(area => area.Id).FirstAsync(cancellationToken);
            int requestTypeId = await dbContext.TiposSolicitud.OrderBy(type => type.Id).Select(type => type.Id).FirstAsync(cancellationToken);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            Solicitud overdueRequest = Solicitud.Crear(
                await codeGenerator.NextAsync(now, cancellationToken),
                "Restablecimiento de acceso",
                "Solicitud de demostración para validar el flujo de atención.",
                PrioridadSolicitud.Alta,
                now.AddDays(-1),
                REQUESTER_ID,
                areaId,
                requestTypeId,
                null,
                now.AddDays(-2));

            Solicitud plannedRequest = Solicitud.Crear(
                await codeGenerator.NextAsync(now, cancellationToken),
                "Mejora de reporte operativo",
                "Solicitud de demostración con fecha de compromiso futura.",
                PrioridadSolicitud.Media,
                now.AddDays(10),
                REQUESTER_ID,
                areaId,
                requestTypeId,
                null,
                now.AddDays(-1));

            dbContext.Solicitudes.AddRange(overdueRequest, plannedRequest);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        LogSeedVerified(logger, null);
    }

    private Usuario CreateUser(Guid id, string name, string email, RolUsuario role, string password)
    {
        Usuario user = new(id, name, email, role);
        user.EstablecerPasswordHash(passwords.Hash(user, password));
        return user;
    }
}

public static class DatabaseInitializationExtensions
{
    public static async Task InitialiseDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        DatabaseInitializer initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitialiseAsync(cancellationToken);
    }
}
