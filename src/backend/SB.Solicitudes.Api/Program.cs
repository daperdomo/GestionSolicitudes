using System.Globalization;
using System.Text.Json.Serialization;
using SB.Solicitudes.Api.Configuration;
using SB.Solicitudes.Api.Middleware;
using SB.Solicitudes.Api.Notifications;
using SB.Solicitudes.Application;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Infrastructure;
using SB.Solicitudes.Infrastructure.Persistence;
using SB.Solicitudes.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        string logsDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "logs");
        Directory.CreateDirectory(logsDirectory);

        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                Path.Combine(logsDirectory, "sb-solicitudes-.log"),
                formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 30,
                shared: true);
    });

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<ApiExceptionHandler>();
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSignalR();
    builder.Services.AddJwtSwagger();
    builder.Services.AddApiSecurity(builder.Configuration);
    string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath)
        .AddPlatformServices(builder.Configuration);
    builder.Services.AddScoped<INotificationDispatcher, SignalRNotificationDispatcher>();

    WebApplication app = builder.Build();

    if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        await app.Services.InitialiseDatabaseAsync();
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseCors("Frontend");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationsHub>("/hubs/notificaciones");
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
        .WithName("Health")
        .AllowAnonymous();

    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "La API terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
