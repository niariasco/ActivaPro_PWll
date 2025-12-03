using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Historial_Tickets")]
    public class HistorialTickets
    {
        [Key]
        [Column("id_historial")]
        public int IdHistorial { get; set; }

        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("accion")]
        public string Accion { get; set; } = null!;

        [Column("estado_anterior")]
        public string? EstadoAnterior { get; set; }

        [Column("estado_nuevo")]
        public string? EstadoNuevo { get; set; }

        [Column("comentario")]
        public string? Comentario { get; set; }

        [Column("fecha_accion")]
        public DateTime? FechaAccion { get; set; }

        [ForeignKey("IdTicket")]
        public virtual Tickets IdTicketNavigation { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;

        // Relación con imágenes de evidencia
        public virtual ICollection<Imagenes_Historial_Tickets>? ImagenesEvidencia { get; set; }
    }
}