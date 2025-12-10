using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Historial_Tickets")]
    public class Historial_Tickets
    {
        [Key]
        [Column("id_historial")]
        public int IdHistorial { get; set; }

        [Required]
        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("accion")]
        [StringLength(255)]  // ⭐ Según tu BD: nvarchar(255)
        public string Accion { get; set; } = null!;

        [Column("estado_anterior")]
        [StringLength(50)]   // ⭐ Según tu BD: nvarchar(50), nullable
        public string? EstadoAnterior { get; set; }

        [Column("estado_nuevo")]
        [StringLength(50)]   // ⭐ Según tu BD: nvarchar(50), nullable
        public string? EstadoNuevo { get; set; }

        [Column("comentario")]
        [StringLength(500)]  // ⭐ Según tu BD: nvarchar(500), nullable
        public string? Comentario { get; set; }

        [Column("fecha_accion")]
        public DateTime? FechaAccion { get; set; }  // ⭐ Según tu BD: datetime, nullable

        // ========== NAVEGACIONES ==========

        [ForeignKey("IdTicket")]
        public virtual Tickets? IdTicketNavigation { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuarios? IdUsuarioNavigation { get; set; }

        // ⚠️ IMPORTANTE: Comentado porque la tabla Imagenes_Historial_Tickets NO existe
        // Si en el futuro la creas, descomenta esto:
        // public virtual ICollection<Imagenes_Historial_Tickets>? ImagenesEvidencia { get; set; }
    }
}