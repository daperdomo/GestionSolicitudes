using Microsoft.Extensions.DependencyInjection;

namespace SB.Solicitudes.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
