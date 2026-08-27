using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Infrastructure.Persistence;
using SB.Solicitudes.Application.Usuarios;

namespace SB.Solicitudes.Infrastructure.Repositories;

internal sealed class UsuarioRepository(ApplicationDbContext dbContext) : IUsuarioRepository
{
    public async Task<Usuario?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        await dbContext.Usuarios.SingleOrDefaultAsync(
            user => user.Correo == normalizedEmail,
            cancellationToken);

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Usuarios.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<UserOption>> GetActiveAnalystsAsync(CancellationToken cancellationToken) =>
        await dbContext.Usuarios.AsNoTracking()
            .Where(user => user.Activo && user.Rol == RolUsuario.Analista)
            .OrderBy(user => user.Nombre)
            .Select(user => new UserOption(user.Id, user.Nombre, user.Correo))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<UserOption>> SearchActiveAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        IQueryable<Usuario> query = dbContext.Usuarios.AsNoTracking().Where(user => user.Activo);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(user => user.Nombre.Contains(term) || user.Correo.Contains(term));
        }

        return await query
            .OrderBy(user => user.Nombre)
            .Take(50)
            .Select(user => new UserOption(user.Id, user.Nombre, user.Correo))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UsuarioListItem>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Usuarios.AsNoTracking()
            .OrderBy(user => user.Nombre)
            .Select(user => new UsuarioListItem(
                user.Id,
                user.Nombre,
                user.Correo,
                user.Rol,
                user.Activo,
                user.FechaCreacion))
            .ToListAsync(cancellationToken);

    public async Task<bool> EmailExistsAsync(
        string normalizedEmail,
        Guid? excludedUserId,
        CancellationToken cancellationToken) =>
        await dbContext.Usuarios.AnyAsync(
            user => user.Correo == normalizedEmail
                && (!excludedUserId.HasValue || user.Id != excludedUserId.Value),
            cancellationToken);

    public async Task AddAsync(Usuario user, CancellationToken cancellationToken) =>
        await dbContext.Usuarios.AddAsync(user, cancellationToken);
}
