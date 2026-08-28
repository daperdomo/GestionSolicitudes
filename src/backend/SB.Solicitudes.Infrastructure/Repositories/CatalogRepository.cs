using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Domain.Entities;
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

    public async Task<IReadOnlyCollection<CatalogAdminItem>> GetAllAreasAsync(CancellationToken cancellationToken) =>
        await dbContext.Areas.AsNoTracking()
            .OrderBy(area => area.Nombre)
            .Select(area => new CatalogAdminItem(area.Id, area.Nombre, area.Activa))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<CatalogAdminItem>> GetAllTiposSolicitudAsync(CancellationToken cancellationToken) =>
        await dbContext.TiposSolicitud.AsNoTracking()
            .OrderBy(type => type.Nombre)
            .Select(type => new CatalogAdminItem(type.Id, type.Nombre, type.Activo))
            .ToListAsync(cancellationToken);

    public Task<Area?> GetAreaForUpdateAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Areas.SingleOrDefaultAsync(area => area.Id == id, cancellationToken);

    public Task<TipoSolicitud?> GetTipoSolicitudForUpdateAsync(int id, CancellationToken cancellationToken) =>
        dbContext.TiposSolicitud.SingleOrDefaultAsync(type => type.Id == id, cancellationToken);

    public Task<bool> AreaNameExistsAsync(string nombre, int? excludedId, CancellationToken cancellationToken) =>
        dbContext.Areas.AnyAsync(
            area => area.Nombre == nombre && (!excludedId.HasValue || area.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> TipoSolicitudNameExistsAsync(string nombre, int? excludedId, CancellationToken cancellationToken) =>
        dbContext.TiposSolicitud.AnyAsync(
            type => type.Nombre == nombre && (!excludedId.HasValue || type.Id != excludedId.Value),
            cancellationToken);

    public async Task AddAreaAsync(Area area, CancellationToken cancellationToken) =>
        await dbContext.Areas.AddAsync(area, cancellationToken);

    public async Task AddTipoSolicitudAsync(TipoSolicitud tipoSolicitud, CancellationToken cancellationToken) =>
        await dbContext.TiposSolicitud.AddAsync(tipoSolicitud, cancellationToken);
}
