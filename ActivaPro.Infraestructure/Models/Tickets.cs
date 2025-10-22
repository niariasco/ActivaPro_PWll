using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models;

[Table("Tickets")]
public partial class Tickets
{
    [Key]
    [Column("id_ticket")]
    public int IdTicket { get; set; }

    [Column("titulo")]
    public string Titulo { get; set; }

    [Column("descripcion")]
    public string Descripcion { get; set; }

    [Column("id_usuario_solicitante")]
    public int IdUsuarioSolicitante { get; set; }

    [Column("id_usuario_asignado")]
    public int? IdUsuarioAsignado { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente";

    [Column("id_valoracion")]
    public int? IdValoracion { get; set; }

    [Column("id_categoria")]
    public int? IdCategoria { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    [Column("id_sla")]
    public int? IdSla { get; set; }

    [Column("fecha_limite_resolucion")]
    public DateTime? FechaLimiteResolucion { get; set; }

    // Relaciones (FK)
    [ForeignKey("IdUsuarioSolicitante")]
    public virtual Usuarios UsuarioSolicitante { get; set; }

    [ForeignKey("IdUsuarioAsignado")]
    public virtual Usuarios UsuarioAsignado { get; set; }

    [ForeignKey("IdCategoria")]
    public virtual Categorias Categoria { get; set; }

    [ForeignKey("IdSla")]
    public virtual SLA_Tickets SLA { get; set; }

    // Colecciones
    public virtual ICollection<Imagenes_Tickets> Imagenes { get; set; }
    public virtual ICollection<Historial_Tickets> Historial { get; set; }
    public virtual ICollection<Valoracion_Tickets> Valoraciones { get; set; }
}