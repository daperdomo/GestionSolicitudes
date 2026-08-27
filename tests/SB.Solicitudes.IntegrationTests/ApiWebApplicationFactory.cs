using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SB.Solicitudes.Infrastructure.Persistence;

namespace SB.Solicitudes.IntegrationTests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"SbSolicitudesTests_{Guid.NewGuid():N}";
    private bool databaseDeleted;
    private bool hostStarted;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("Jwt:Issuer", "SB.Solicitudes.Api.Tests");
        builder.UseSetting("Jwt:Audience", "SB.Solicitudes.IntegrationTests");
        builder.UseSetting("Jwt:SigningKey", "INTEGRATION_TEST_SIGNING_KEY_NOT_FOR_PRODUCTION");
        builder.UseSetting("Jwt:ExpirationMinutes", "5");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);
        hostStarted = true;
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && hostStarted && !databaseDeleted)
        {
            databaseDeleted = true;
            using IServiceScope scope = Services.CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureDeleted();
        }

        base.Dispose(disposing);
    }
}
