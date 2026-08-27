using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SB.Solicitudes.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DEVELOPMENT_CONNECTION =
        "Server=(localdb)\\MSSQLLocalDB;Database=SbSolicitudes;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(DEVELOPMENT_CONNECTION)
            .Options;

        return new ApplicationDbContext(options);
    }
}
