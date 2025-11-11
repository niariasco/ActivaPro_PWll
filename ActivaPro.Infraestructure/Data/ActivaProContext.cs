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

    public DbSet<Categorias> Categorias { get; set; }
    public DbSet<Etiquetas> Etiquetas { get; set; }

    public DbSet<Tecnico_Especialidad> Tecnico_Especialidad { get; set; }
    public virtual DbSet<EspecialidadesU> EspecialidadesU { get; set; }
    public DbSet<Categoria_Etiqueta> Categoria_Etiqueta { get; set; }
    public DbSet<Categoria_Especialidad> Categoria_Especialidad { get; set; }
    public DbSet<Categoria_SLA> Categoria_SLA { get; set; }

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

        // Categorias
        modelBuilder.Entity<Categorias>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(e => e.id_categoria);
            entity.Property(e => e.id_categoria).HasColumnName("id_categoria");
            entity.Property(e => e.nombre_categoria).HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<Etiquetas>(entity =>
        {
            entity.ToTable("Etiquetas");
            entity.HasKey(e => e.id_etiqueta);
            entity.Property(e => e.id_etiqueta).HasColumnName("id_etiqueta");
            entity.Property(e => e.nombre_etiqueta).HasColumnName("nombre_etiqueta");
        });

        modelBuilder.Entity<Especialidades>(entity =>
        {
            entity.ToTable("Especialidades");
            entity.HasKey(e => e.id_especialidad);
            entity.Property(e => e.id_especialidad).HasColumnName("id_especialidad");
            entity.Property(e => e.NombreEspecialidad).HasColumnName("nombre_especialidad");
        });

        modelBuilder.Entity<SLA_Tickets>(entity =>
        {
            entity.ToTable("SLA_Tickets");
            entity.HasKey(e => e.id_sla);
            entity.Property(e => e.id_sla).HasColumnName("id_sla");
            entity.Property(e => e.descripcion).HasColumnName("descripcion");
            entity.Property(e => e.prioridad).HasColumnName("prioridad");
        });

        // Join: Categoria_Etiqueta
        modelBuilder.Entity<Categoria_Etiqueta>(entity =>
        {
            entity.ToTable("Categoria_Etiqueta");
            entity.HasKey(e => new { e.id_categoria, e.id_etiqueta });

            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.CategoriaEtiquetas)
                  .HasForeignKey(e => e.id_categoria)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Etiqueta)
                  .WithMany()
                  .HasForeignKey(e => e.id_etiqueta)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Join: Categoria_Especialidad
        modelBuilder.Entity<Categoria_Especialidad>(entity =>
        {
            entity.ToTable("Categoria_Especialidad");
            entity.HasKey(e => new { e.id_categoria, e.id_especialidad });

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

            entity.HasOne(e => e.Especialidad)
                  .WithMany()
                  .HasForeignKey(e => e.id_especialidad)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Join: Categoria_SLA
        modelBuilder.Entity<Categoria_SLA>(entity =>
        {
            entity.ToTable("Categoria_SLA");
            entity.HasKey(e => new { e.id_categoria, e.id_sla });

            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.CategoriaSLAs)
                  .HasForeignKey(e => e.id_categoria)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SLA)
                  .WithMany()
                  .HasForeignKey(e => e.id_sla)
                  .OnDelete(DeleteBehavior.Restrict);
        });

   
    // Tecinicos
    modelBuilder.Entity<Tecnicos>(entity =>
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

            //  entity.Property(e => e.IdTecnico).HasColumnName("idTecnico");
            // FK a Usuario
            //  entity.Property(e => e.IdUsuario).HasColumnName("idUsuario").IsRequired();
          
            entity.Property(e => e.CargaTrabajo).HasColumnName("cargaTrabajo").IsRequired();
            entity.Property(e => e.Disponible).HasColumnName("disponible") .IsRequired().HasDefaultValue(true);
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

        modelBuilder.Entity<EspecialidadesU>(entity =>
        {
            entity.ToTable("EspecialidadesU");
            entity.HasKey(e => e.IdEspecialidadesU);
            entity.Property(e => e.IdEspecialidadesU).HasColumnName("id_especialidadesU");
            entity.Property(e => e.NombreEspecialidadU).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        });



        // Join Tecnico_Especialidad apuntando a EspecialidadesU
        modelBuilder.Entity<Tecnico_Especialidad>(entity =>
        {
            entity.HasKey(e => new { e.IdTecnico, e.IdEspecialidadesU });

            entity.HasOne(e => e.Tecnico)
                  .WithMany()
                  .HasForeignKey(e => e.IdTecnico)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.EspecialidadU)
                  .WithMany()
                  .HasForeignKey(e => e.IdEspecialidadesU)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Usuario_Rol (join)
        modelBuilder.Entity<UsuarioRol>(entity =>
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

        modelBuilder.Entity<Tecnicos>(entity =>
        {
            entity.HasKey(e => e.IdTecnico).HasName("PK__Tecnicos__BF289893228DDE83");

            entity.Property(e => e.Disponible).HasDefaultValue(true);
            entity.Property(e => e.Especialidades).HasMaxLength(200);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Tecnicos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tecnicos_Usuarios");
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
