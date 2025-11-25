using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Notificaciones")]
    public class Notificacion
    {
        [Key]
        [Column("id_notificacion")]
        public int IdNotificacion { get; set; }

        [Column("id_ticket")]
        public int? IdTicket { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(2000)]
        [Column("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("accion")]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [Column("leido")]
        public bool Leido { get; set; }

        [Required]
        [Column("fecha_envio")]
        public DateTime FechaEnvio { get; set; } // sin inicialización UTC
    }
}