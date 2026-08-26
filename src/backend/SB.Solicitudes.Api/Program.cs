using System.Globalization;
using SB.Solicitudes.Application;
using SB.Solicitudes.Infrastructure;
using SB.Solicitudes.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

    builder.Services.AddProblemDetails();
    builder.Services.AddControllers();
    builder.Services.AddAuthorization();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services
        .AddApplication()
        .AddInfrastructure()
        .AddPlatformServices();

    WebApplication app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
        .WithName("Health")
        .AllowAnonymous();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "La API terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
