using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;

namespace SB.Solicitudes.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<TipoSolicitud> TiposSolicitud => Set<TipoSolicitud>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();
    public DbSet<HistorialAsignacion> HistorialAsignaciones => Set<HistorialAsignacion>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<ActividadSolicitud> ActividadesSolicitud => Set<ActividadSolicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasSequence<long>("SolicitudCodigoSequence")
            .StartsAt(1)
            .IncrementsBy(1);
    }
}
