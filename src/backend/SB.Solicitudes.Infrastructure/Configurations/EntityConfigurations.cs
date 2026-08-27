using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Rules;

namespace SB.Solicitudes.Infrastructure.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Nombre).HasMaxLength(FieldLengths.UsuarioNombre).IsRequired();
        builder.Property(user => user.Correo).HasMaxLength(FieldLengths.UsuarioCorreo).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.Rol).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(user => user.Correo).IsUnique();
    }
}

internal sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");
        builder.HasKey(area => area.Id);
        builder.Property(area => area.Nombre).HasMaxLength(FieldLengths.CatalogoNombre).IsRequired();
        builder.HasIndex(area => area.Nombre).IsUnique();
    }
}

internal sealed class TipoSolicitudConfiguration : IEntityTypeConfiguration<TipoSolicitud>
{
    public void Configure(EntityTypeBuilder<TipoSolicitud> builder)
    {
        builder.ToTable("TiposSolicitud");
        builder.HasKey(type => type.Id);
        builder.Property(type => type.Nombre).HasMaxLength(FieldLengths.CatalogoNombre).IsRequired();
        builder.HasIndex(type => type.Nombre).IsUnique();
    }
}

internal sealed class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable("Solicitudes");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Codigo).HasMaxLength(FieldLengths.SolicitudCodigo).IsRequired();
        builder.Property(request => request.Titulo).HasMaxLength(FieldLengths.SolicitudTitulo).IsRequired();
        builder.Property(request => request.Descripcion).HasMaxLength(FieldLengths.SolicitudDescripcion).IsRequired();
        builder.Property(request => request.EvidenciaReferencia).HasMaxLength(FieldLengths.EvidenciaReferencia);
        builder.Property(request => request.Prioridad).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(request => request.Estado).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(request => request.RowVersion).IsRowVersion();

        builder.HasIndex(request => request.Codigo).IsUnique();
        builder.HasIndex(request => request.Estado);
        builder.HasIndex(request => request.Prioridad);
        builder.HasIndex(request => request.AreaId);
        builder.HasIndex(request => request.UsuarioSolicitanteId);
        builder.HasIndex(request => request.ResponsableId);
        builder.HasIndex(request => request.FechaCreacion);

        builder.HasOne(request => request.UsuarioSolicitante)
            .WithMany()
            .HasForeignKey(request => request.UsuarioSolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.Responsable)
            .WithMany()
            .HasForeignKey(request => request.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.Area)
            .WithMany()
            .HasForeignKey(request => request.AreaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.TipoSolicitud)
            .WithMany()
            .HasForeignKey(request => request.TipoSolicitudId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(request => request.HistorialEstados)
            .WithOne()
            .HasForeignKey(history => history.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(request => request.HistorialAsignaciones)
            .WithOne()
            .HasForeignKey(history => history.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(request => request.Comentarios)
            .WithOne()
            .HasForeignKey(comment => comment.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(request => request.Actividades)
            .WithOne()
            .HasForeignKey(activity => activity.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ActividadSolicitudConfiguration : IEntityTypeConfiguration<ActividadSolicitud>
{
    public void Configure(EntityTypeBuilder<ActividadSolicitud> builder)
    {
        builder.ToTable("ActividadesSolicitud");
        builder.HasKey(activity => activity.Id);
        builder.Property(activity => activity.Campo).HasMaxLength(FieldLengths.ActividadCampo).IsRequired();
        builder.Property(activity => activity.ValorAnterior).HasMaxLength(FieldLengths.ActividadValor);
        builder.Property(activity => activity.ValorNuevo).HasMaxLength(FieldLengths.ActividadValor);
        builder.HasIndex(activity => new { activity.SolicitudId, activity.Fecha });
        builder.HasOne(activity => activity.Usuario)
            .WithMany()
            .HasForeignKey(activity => activity.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HistorialEstadoConfiguration : IEntityTypeConfiguration<HistorialEstado>
{
    public void Configure(EntityTypeBuilder<HistorialEstado> builder)
    {
        builder.ToTable("HistorialEstados");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.EstadoAnterior).HasConversion<string>().HasMaxLength(40);
        builder.Property(history => history.EstadoNuevo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(history => history.Comentario).HasMaxLength(FieldLengths.HistorialComentario);
        builder.HasIndex(history => new { history.SolicitudId, history.Fecha });
        builder.HasOne(history => history.Usuario)
            .WithMany()
            .HasForeignKey(history => history.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HistorialAsignacionConfiguration : IEntityTypeConfiguration<HistorialAsignacion>
{
    public void Configure(EntityTypeBuilder<HistorialAsignacion> builder)
    {
        builder.ToTable("HistorialAsignaciones");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Comentario).HasMaxLength(FieldLengths.HistorialComentario);
        builder.HasIndex(history => new { history.SolicitudId, history.Fecha });
        builder.HasOne(history => history.ResponsableAnterior)
            .WithMany()
            .HasForeignKey(history => history.ResponsableAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(history => history.ResponsableNuevo)
            .WithMany()
            .HasForeignKey(history => history.ResponsableNuevoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(history => history.Usuario)
            .WithMany()
            .HasForeignKey(history => history.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentarios");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Texto).HasMaxLength(FieldLengths.ComentarioTexto).IsRequired();
        builder.Property(comment => comment.Visibilidad).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(comment => new { comment.SolicitudId, comment.Fecha });
        builder.HasOne(comment => comment.Usuario)
            .WithMany()
            .HasForeignKey(comment => comment.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Asunto).HasMaxLength(FieldLengths.NotificacionAsunto).IsRequired();
        builder.Property(notification => notification.Mensaje).HasMaxLength(FieldLengths.NotificacionMensaje).IsRequired();
        builder.Property(notification => notification.Canal).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(notification => notification.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(notification => new { notification.DestinatarioId, notification.Estado });
        builder.HasIndex(notification => new { notification.DestinatarioId, notification.FechaLectura });
        builder.HasIndex(notification => new { notification.DestinatarioId, notification.FechaCreacion });
        builder.HasOne(notification => notification.Solicitud)
            .WithMany()
            .HasForeignKey(notification => notification.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(notification => notification.Destinatario)
            .WithMany()
            .HasForeignKey(notification => notification.DestinatarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
