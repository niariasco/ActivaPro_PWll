using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    // Unifica a UN solo modelo (elimina cualquier archivo duplicado Notificaciones.cs)
    [Table("Notificaciones")]
    public class Notificacion
    {
        // La columna en BD es INT (no BIGINT); usar int para evitar InvalidCastException
        [Key]
        [Column("id_notificacion")]
        public int IdNotificacion { get; set; }  // antes era long -> causaba cast de int a long

        [Column("id_ticket")]
        public int? IdTicket { get; set; }       // Permitir null para notificaciones de Login

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
        public string Accion { get; set; } = string.Empty; // TicketStateChange | Login

        [Required]
        [Column("leido")]
        public bool Leido { get; set; }

        [Required]
        [Column("fecha_envio")]
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}