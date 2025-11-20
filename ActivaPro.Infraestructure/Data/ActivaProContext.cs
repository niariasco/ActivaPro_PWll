using ActivaPro.Infraestructure.Enums;
using ActivaPro.Infraestructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ActivaPro.Infraestructure.Data;

public partial class ActivaProContext : DbContext
{
    public ActivaProContext(DbContextOptions<ActivaProContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Tecnicos> Tecnico { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }
    public virtual DbSet<Roles> Roles { get; set; } 
    public virtual DbSet<UsuarioRol> UsuarioRoles { get; set; }
    public virtual DbSet<Especialidades> Especialidades { get; set; }
    public virtual DbSet<SLA_Tickets> SLA_Tickets { get; set; }
    public virtual DbSet<Tickets> Ticketes { get; set; }
    public virtual DbSet<AsignacionesTickets> AsignacionesTickets { get; set; }

    public DbSet<Categorias> Categorias { get; set; }
    public DbSet<Etiquetas> Etiquetas { get; set; }

    public DbSet<Tecnico_Especialidad> Tecnico_Especialidad { get; set; }
    public virtual DbSet<EspecialidadesU> EspecialidadesU { get; set; }
    public DbSet<Categoria_Etiqueta> Categoria_Etiqueta { get; set; }
    public DbSet<Categoria_Especialidad> Categoria_Especialidad { get; set; }
    public DbSet<Categoria_SLA> Categoria_SLA { get; set; }
    public virtual DbSet<Historial_Tickets> Historial_Tickets { get; set; }
    public virtual DbSet<Imagenes_Tickets> Imagenes_Tickets { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Usuarios
        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.Property(e => e.UltimoInicioSesion).HasColumnName("ultimo_inicio_sesion").HasColumnType("datetime");
        });
        //notificaciones
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasIndex(n => new { n.IdUsuario, n.Leido });
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

            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.CategoriaEspecialidades)
                  .HasForeignKey(e => e.id_categoria)
                  .OnDelete(DeleteBehavior.Cascade);

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
            entity.ToTable("Tecnicos");

            entity.HasKey(e => e.IdTecnico);

            //  entity.Property(e => e.IdTecnico).HasColumnName("idTecnico");
            // FK a Usuario
            //  entity.Property(e => e.IdUsuario).HasColumnName("idUsuario").IsRequired();
          
            entity.Property(e => e.CargaTrabajo).HasColumnName("cargaTrabajo").IsRequired();
            entity.Property(e => e.Disponible).HasColumnName("disponible") .IsRequired().HasDefaultValue(true);
        });

        // Roles
        modelBuilder.Entity<Roles>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(e => e.IdRol);

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.NombreRol).HasColumnName("nombre_rol").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
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
            entity.ToTable("Usuario_Rol");

            // PK compuesto (id_usuario, id_rol)
            entity.HasKey(e => new { e.IdUsuario, e.IdRol });

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.UsuarioRoles)
                  .HasForeignKey(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_UsuarioRol_Usuario");

            entity.HasOne(e => e.Rol)
                  .WithMany(r => r.UsuarioRoles)
                  .HasForeignKey(e => e.IdRol)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_UsuarioRol_Rol");
        });

        // Especialidades
        modelBuilder.Entity<Especialidades>(entity =>
        {
            entity.ToTable("Especialidades");

            entity.HasKey(e => e.id_especialidad);

            entity.Property(e => e.id_especialidad).HasColumnName("id_especialidad");
            entity.Property(e => e.NombreEspecialidad).HasColumnName("nombre_especialidad").HasMaxLength(100).IsRequired();
        });


        modelBuilder.Entity<Tickets>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(e => e.IdTicket);

            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.Titulo).HasColumnName("titulo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.IdUsuarioSolicitante).HasColumnName("id_usuario_solicitante");
            entity.Property(e => e.IdUsuarioAsignado).HasColumnName("id_usuario_asignado");
            entity.Property(e => e.Estado).HasColumnName("estado");  // Sin ValueConverter
            entity.Property(e => e.IdValoracion).HasColumnName("id_valoracion");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.IdSla).HasColumnName("id_sla");
            entity.Property(e => e.FechaLimiteResolucion).HasColumnName("fecha_limite_resolucion");

            // Configurar relaciones
            entity.HasOne(t => t.UsuarioSolicitante)
                .WithMany()
                .HasForeignKey(t => t.IdUsuarioSolicitante)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(t => t.UsuarioAsignado)
                .WithMany()
                .HasForeignKey(t => t.IdUsuarioAsignado)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(t => t.Categoria)
                .WithMany()
                .HasForeignKey(t => t.IdCategoria)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(t => t.SLA)
                .WithMany()
                .HasForeignKey(t => t.IdSla)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AsignacionesTickets>(entity =>
        {
            entity.ToTable("Asignaciones_Tickets");

            entity.HasKey(e => e.IdAsignacion);

            entity.Property(e => e.IdAsignacion)
                .HasColumnName("id_asignacion")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdTicket)
                .HasColumnName("id_ticket");

            entity.Property(e => e.IdUsuarioAsignado)
                .HasColumnName("id_usuario_asignado");

            entity.Property(e => e.IdUsuarioAsignador)
                .HasColumnName("id_usuario_asignador");

            entity.Property(e => e.TipoAsignacion)
                .HasColumnName("tipo_asignacion")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Manual");

            entity.Property(e => e.FechaAsignacion)
                .HasColumnName("fecha_asignacion")
                .HasDefaultValueSql("GETDATE()");

            // Relaciones
            entity.HasOne(d => d.IdTicketNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Asignaciones_Tickets_Tickets");

            entity.HasOne(d => d.IdUsuarioAsignadoNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioAsignado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asignaciones_Tickets_Usuario_Asignado");

            entity.HasOne(d => d.IdUsuarioAsignadorNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioAsignador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asignaciones_Tickets_Usuario_Asignador");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}