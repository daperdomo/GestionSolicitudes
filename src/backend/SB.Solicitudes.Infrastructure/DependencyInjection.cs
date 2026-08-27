using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Dashboard;
using SB.Solicitudes.Application.EntidadesGubernamentales;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Infrastructure.Persistence;
using SB.Solicitudes.Infrastructure.Repositories;
using SB.Solicitudes.Infrastructure.GovernmentEntities;

namespace SB.Solicitudes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se configuró ConnectionStrings:DefaultConnection.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<ISolicitudRepository, SolicitudRepository>();
        services.AddScoped<ISolicitudCodeGenerator, SqlSolicitudCodeGenerator>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<DatabaseInitializer>();

        string relativeFilePath = configuration["GovernmentEntities:FilePath"]
            ?? "App_Data/entidades-gubernamentales.json";
        string governmentEntityFilePath = Path.GetFullPath(Path.Combine(contentRootPath, relativeFilePath));
        if (!governmentEntityFilePath.StartsWith(Path.GetFullPath(contentRootPath), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La ruta de entidades gubernamentales debe permanecer dentro del proyecto API.");
        }

        services.AddSingleton(new GovernmentEntityFileOptions(governmentEntityFilePath));
        services.AddSingleton<IGovernmentEntityRepository, TextFileGovernmentEntityRepository>();

        return services;
    }
}
