using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Application.Dashboard;
using SB.Solicitudes.Application.EntidadesGubernamentales;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Application.Usuarios;

namespace SB.Solicitudes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection)));

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IGovernmentEntityService, GovernmentEntityService>();
        services.AddScoped<ISolicitudService, SolicitudService>();
        services.AddScoped<UsuarioAdministrationService>();
        services.AddScoped<IUsuarioAdministrationService>(provider =>
            provider.GetRequiredService<UsuarioAdministrationService>());
        services.AddScoped<IUsuarioRegistrationService>(provider =>
            provider.GetRequiredService<UsuarioAdministrationService>());

        return services;
    }
}
