using System;
using System.Collections.Generic;
using ActivaPro.Infraestructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Infraestructure.Data;

public partial class ActivaProContext : DbContext
{
    public ActivaProContext(DbContextOptions<ActivaProContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsignacionesTickets> AsignacionesTickets { get; set; }

    public virtual DbSet<BitacoraUsuarios> BitacoraUsuarios { get; set; }

    public virtual DbSet<Categorias> Categorias { get; set; }

    public virtual DbSet<Especialidades> Especialidades { get; set; }

    public virtual DbSet<Etiquetas> Etiquetas { get; set; }

    public virtual DbSet<HistorialTickets> HistorialTickets { get; set; }

    public virtual DbSet<ImagenesTickets> ImagenesTickets { get; set; }

    public virtual DbSet<Notificaciones> Notificaciones { get; set; }

    public virtual DbSet<ReglasAutotriage> ReglasAutotriage { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<SlaTickets> SlaTickets { get; set; }

    public virtual DbSet<Tickets> Tickets { get; set; }

    public virtual DbSet<UsuarioRol> UsuarioRol { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<ValoracionNotificaciones> ValoracionNotificaciones { get; set; }

    public virtual DbSet<ValoracionTickets> ValoracionTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsignacionesTickets>(entity =>
        {
            entity.HasKey(e => e.IdAsignacion).HasName("PK__Asignaci__C3F7F966D9042EE3");

            entity.ToTable("Asignaciones_Tickets");

            entity.Property(e => e.IdAsignacion).HasColumnName("id_asignacion");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdUsuarioAsignado).HasColumnName("id_usuario_asignado");
            entity.Property(e => e.IdUsuarioAsignador).HasColumnName("id_usuario_asignador");
            entity.Property(e => e.TipoAsignacion)
                .HasMaxLength(20)
                .HasDefaultValue("Manual")
                .HasColumnName("tipo_asignacion");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.AsignacionesTickets)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("FK__Asignacio__id_ti__6FE99F9F");

            entity.HasOne(d => d.IdUsuarioAsignadoNavigation).WithMany(p => p.AsignacionesTicketsIdUsuarioAsignadoNavigation)
                .HasForeignKey(d => d.IdUsuarioAsignado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Asignacio__id_us__70DDC3D8");

            entity.HasOne(d => d.IdUsuarioAsignadorNavigation).WithMany(p => p.AsignacionesTicketsIdUsuarioAsignadorNavigation)
                .HasForeignKey(d => d.IdUsuarioAsignador)
                .HasConstraintName("FK__Asignacio__id_us__71D1E811");
        });

        modelBuilder.Entity<BitacoraUsuarios>(entity =>
        {
            entity.HasKey(e => e.IdBitacora).HasName("PK__Bitacora__7E4268B08487D90F");

            entity.ToTable("Bitacora_Usuarios");

            entity.Property(e => e.IdBitacora).HasColumnName("id_bitacora");
            entity.Property(e => e.Accion)
                .HasMaxLength(255)
                .HasColumnName("accion");
            entity.Property(e => e.FechaAccion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_accion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.BitacoraUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bitacora___id_us__75A278F5");
        });

        modelBuilder.Entity<Categorias>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__CD54BC5AA3A1D479");

            entity.HasIndex(e => e.NombreCategoria, "UQ__Categori__4EBF6259C3E8B874").IsUnique();

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(100)
                .HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<Especialidades>(entity =>
        {
            entity.HasKey(e => e.IdEspecialidad).HasName("PK__Especial__C1D1376323DBCD6E");

            entity.HasIndex(e => e.NombreEspecialidad, "UQ__Especial__B08A68E45CEC4D64").IsUnique();

            entity.Property(e => e.IdEspecialidad).HasColumnName("id_especialidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreEspecialidad)
                .HasMaxLength(100)
                .HasColumnName("nombre_especialidad");
        });

        modelBuilder.Entity<Etiquetas>(entity =>
        {
            entity.HasKey(e => e.IdEtiqueta).HasName("PK__Etiqueta__FA0DD2AD726F750F");

            entity.HasIndex(e => e.NombreEtiqueta, "UQ__Etiqueta__3F48E4F15892B0BE").IsUnique();

            entity.Property(e => e.IdEtiqueta).HasColumnName("id_etiqueta");
            entity.Property(e => e.NombreEtiqueta)
                .HasMaxLength(50)
                .HasColumnName("nombre_etiqueta");
        });

        modelBuilder.Entity<HistorialTickets>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PK__Historia__76E6C502BC87DC39");

            entity.ToTable("Historial_Tickets");

            entity.Property(e => e.IdHistorial).HasColumnName("id_historial");
            entity.Property(e => e.Accion)
                .HasMaxLength(255)
                .HasColumnName("accion");
            entity.Property(e => e.FechaAccion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_accion");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.HistorialTickets)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("FK__Historial__id_ti__6383C8BA");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialTickets)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__id_us__6477ECF3");
        });

        modelBuilder.Entity<ImagenesTickets>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__Imagenes__27CC2689C3F63172");

            entity.ToTable("Imagenes_Tickets");

            entity.Property(e => e.IdImagen).HasColumnName("id_imagen");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .HasColumnName("ruta_archivo");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.ImagenesTickets)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("FK__Imagenes___id_ti__5FB337D6");
        });

        modelBuilder.Entity<Notificaciones>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__Notifica__8270F9A52F5D12F3");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.Accion)
                .HasMaxLength(255)
                .HasColumnName("accion");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leido)
                .HasDefaultValue(false)
                .HasColumnName("leido");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(255)
                .HasColumnName("mensaje");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__id_ti__693CA210");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__id_us__6A30C649");
        });

        modelBuilder.Entity<ReglasAutotriage>(entity =>
        {
            entity.HasKey(e => e.IdRegla).HasName("PK__Reglas_A__46D1C19204C806E1");

            entity.ToTable("Reglas_Autotriage");

            entity.Property(e => e.IdRegla).HasColumnName("id_regla");
            entity.Property(e => e.AccionCategoria).HasColumnName("accion_categoria");
            entity.Property(e => e.AccionPrioridad)
                .HasMaxLength(10)
                .HasColumnName("accion_prioridad");
            entity.Property(e => e.AccionUsuario).HasColumnName("accion_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Condicion).HasColumnName("condicion");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.NombreRegla)
                .HasMaxLength(100)
                .HasColumnName("nombre_regla");

            entity.HasOne(d => d.AccionCategoriaNavigation).WithMany(p => p.ReglasAutotriage)
                .HasForeignKey(d => d.AccionCategoria)
                .HasConstraintName("FK__Reglas_Au__accio__02084FDA");

            entity.HasOne(d => d.AccionUsuarioNavigation).WithMany(p => p.ReglasAutotriage)
                .HasForeignKey(d => d.AccionUsuario)
                .HasConstraintName("FK__Reglas_Au__accio__02FC7413");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__6ABCB5E0C2201332");

            entity.HasIndex(e => e.NombreRol, "UQ__Roles__673CB43570B60B0F").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<SlaTickets>(entity =>
        {
            entity.HasKey(e => e.IdSla).HasName("PK__SLA_Tick__6D6C1A3A6096A290");

            entity.ToTable("SLA_Tickets");

            entity.Property(e => e.IdSla).HasColumnName("id_sla");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Prioridad)
                .HasMaxLength(10)
                .HasColumnName("prioridad");
            entity.Property(e => e.TiempoResolucionHoras).HasColumnName("tiempo_resolucion_horas");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.SlaTickets)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__SLA_Ticke__id_ca__49C3F6B7");
        });

        modelBuilder.Entity<Tickets>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PK__Tickets__48C6F5233A811456");

            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaLimiteResolucion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_limite_resolucion");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdSla).HasColumnName("id_sla");
            entity.Property(e => e.IdUsuarioAsignado).HasColumnName("id_usuario_asignado");
            entity.Property(e => e.IdUsuarioSolicitante).HasColumnName("id_usuario_solicitante");
            entity.Property(e => e.IdValoracion).HasColumnName("id_valoracion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__Tickets__id_cate__52593CB8");

            entity.HasOne(d => d.IdSlaNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdSla)
                .HasConstraintName("FK__Tickets__id_sla__534D60F1");

            entity.HasOne(d => d.IdUsuarioAsignadoNavigation).WithMany(p => p.TicketsIdUsuarioAsignadoNavigation)
                .HasForeignKey(d => d.IdUsuarioAsignado)
                .HasConstraintName("FK__Tickets__id_usua__5165187F");

            entity.HasOne(d => d.IdUsuarioSolicitanteNavigation).WithMany(p => p.TicketsIdUsuarioSolicitanteNavigation)
                .HasForeignKey(d => d.IdUsuarioSolicitante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__id_usua__5070F446");

            entity.HasMany(d => d.IdEtiqueta).WithMany(p => p.IdTicket)
                .UsingEntity<Dictionary<string, object>>(
                    "TicketEtiqueta",
                    r => r.HasOne<Etiquetas>().WithMany()
                        .HasForeignKey("IdEtiqueta")
                        .HasConstraintName("FK__Ticket_Et__id_et__5BE2A6F2"),
                    l => l.HasOne<Tickets>().WithMany()
                        .HasForeignKey("IdTicket")
                        .HasConstraintName("FK__Ticket_Et__id_ti__5AEE82B9"),
                    j =>
                    {
                        j.HasKey("IdTicket", "IdEtiqueta").HasName("PK__Ticket_E__8766280940558420");
                        j.ToTable("Ticket_Etiqueta");
                        j.IndexerProperty<int>("IdTicket").HasColumnName("id_ticket");
                        j.IndexerProperty<int>("IdEtiqueta").HasColumnName("id_etiqueta");
                    });
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => new { e.IdUsuario, e.IdRol }).HasName("PK__Usuario___5895CFF351B95011");

            entity.ToTable("Usuario_Rol");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_asignacion");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UsuarioRol)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Usuario_R__id_ro__403A8C7D");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioRol)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Usuario_R__id_us__3F466844");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__4E3E04AD15E5094F");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__2A586E0B05C2909F").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroSucursal).HasColumnName("numero_sucursal");

            entity.HasMany(d => d.IdEspecialidad).WithMany(p => p.IdUsuario)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuarioEspecialidad",
                    r => r.HasOne<Especialidades>().WithMany()
                        .HasForeignKey("IdEspecialidad")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Usuario_E__id_es__7C4F7684"),
                    l => l.HasOne<Usuarios>().WithMany()
                        .HasForeignKey("IdUsuario")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Usuario_E__id_us__7B5B524B"),
                    j =>
                    {
                        j.HasKey("IdUsuario", "IdEspecialidad").HasName("PK__Usuario___622317DB9FDB92C7");
                        j.ToTable("Usuario_Especialidad");
                        j.IndexerProperty<int>("IdUsuario").HasColumnName("id_usuario");
                        j.IndexerProperty<int>("IdEspecialidad").HasColumnName("id_especialidad");
                    });
        });

        modelBuilder.Entity<ValoracionNotificaciones>(entity =>
        {
            entity.HasKey(e => e.IdValoracion).HasName("PK__Valoraci__1861B24998B3372B");

            entity.ToTable("Valoracion_Notificaciones");

            entity.Property(e => e.IdValoracion).HasColumnName("id_valoracion");
            entity.Property(e => e.Comentario).HasColumnName("comentario");
            entity.Property(e => e.FechaValoracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_valoracion");
            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Puntaje).HasColumnName("puntaje");

            entity.HasOne(d => d.IdNotificacionNavigation).WithMany(p => p.ValoracionNotificaciones)
                .HasForeignKey(d => d.IdNotificacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Valoracio__id_no__07C12930");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ValoracionNotificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Valoracio__id_us__08B54D69");
        });

        modelBuilder.Entity<ValoracionTickets>(entity =>
        {
            entity.HasKey(e => e.IdValoracion).HasName("PK__Valoraci__1861B2497778364C");

            entity.ToTable("Valoracion_Tickets");

            entity.Property(e => e.IdValoracion).HasColumnName("id_valoracion");
            entity.Property(e => e.Comentario).HasColumnName("comentario");
            entity.Property(e => e.FechaValoracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_valoracion");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.Puntaje).HasColumnName("puntaje");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.ValoracionTickets)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("FK__Valoracio__id_ti__5812160E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
