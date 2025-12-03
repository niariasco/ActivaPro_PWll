using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Asignaciones_Tickets")]
    public partial class AsignacionesTickets
    {
        [Key]
        [Column("id_asignacion")]
        public int IdAsignacion { get; set; }

        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [Column("id_usuario_asignado")]
        public int IdUsuarioAsignado { get; set; }

        [Column("id_usuario_asignador")]
        public int? IdUsuarioAsignador { get; set; }

        [Column("tipo_asignacion")]
        [MaxLength(20)]
        public string TipoAsignacion { get; set; } = "Manual";

        [Column("fecha_asignacion")]
        public DateTime? FechaAsignacion { get; set; }

        
        [Column("puntaje_asignacion")]
        public decimal? PuntajeAsignacion { get; set; }

        [Column("justificacion")]
        [MaxLength(1000)]
        public string? Justificacion { get; set; }

        // Relaciones (FK)
        [ForeignKey("IdTicket")]
        public virtual Tickets IdTicketNavigation { get; set; } = null!;

        [ForeignKey("IdUsuarioAsignado")]
        public virtual Usuarios IdUsuarioAsignadoNavigation { get; set; } = null!;

        [ForeignKey("IdUsuarioAsignador")]
        public virtual Usuarios? IdUsuarioAsignadorNavigation { get; set; }
    }
}