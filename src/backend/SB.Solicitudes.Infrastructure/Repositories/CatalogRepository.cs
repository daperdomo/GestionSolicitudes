using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Infrastructure.Persistence;

namespace SB.Solicitudes.Infrastructure.Repositories;

internal sealed class CatalogRepository(ApplicationDbContext dbContext) : ICatalogRepository
{
    public async Task<bool> AreaExistsAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Areas.AnyAsync(area => area.Id == id && area.Activa, cancellationToken);

    public async Task<bool> TipoSolicitudExistsAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.TiposSolicitud.AnyAsync(type => type.Id == id && type.Activo, cancellationToken);

    public async Task<IReadOnlyCollection<CatalogItem>> GetAreasAsync(CancellationToken cancellationToken) =>
        await dbContext.Areas.AsNoTracking()
            .Where(area => area.Activa)
            .OrderBy(area => area.Nombre)
            .Select(area => new CatalogItem(area.Id, area.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<CatalogItem>> GetTiposSolicitudAsync(CancellationToken cancellationToken) =>
        await dbContext.TiposSolicitud.AsNoTracking()
            .Where(type => type.Activo)
            .OrderBy(type => type.Nombre)
            .Select(type => new CatalogItem(type.Id, type.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<CatalogItem?> GetAreaByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Areas.AsNoTracking()
            .Where(area => area.Id == id && area.Activa)
            .Select(area => new CatalogItem(area.Id, area.Nombre))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CatalogItem?> GetTipoSolicitudByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.TiposSolicitud.AsNoTracking()
            .Where(type => type.Id == id && type.Activo)
            .Select(type => new CatalogItem(type.Id, type.Nombre))
            .SingleOrDefaultAsync(cancellationToken);
}
