using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Services.Authentication;
using SB.Solicitudes.Services.Notifications;

namespace SB.Solicitudes.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => options.SigningKey.Length >= 32, "Jwt:SigningKey debe tener al menos 32 caracteres.")
            .Validate(options => options.ExpirationMinutes > 0, "Jwt:ExpirationMinutes debe ser mayor que cero.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<INotificationDispatcher, LoggingNotificationDispatcher>();

        return services;
    }
}
