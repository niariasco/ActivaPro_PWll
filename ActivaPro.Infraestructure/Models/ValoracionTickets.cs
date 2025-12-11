using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ActivaPro.Infraestructure.Models;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Valoracion_Notificaciones")]
    public class Valoracion_Notificaciones
    {
        [Key]
        [Column("id_valoracion")]
        public int IdValoracion { get; set; }

        [Required]
        [Column("id_notificacion")]
        public int IdNotificacion { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("puntaje")]
        public byte Puntaje { get; set; } // 1 a 5

        [Required]
        [Column("comentario")]
        [StringLength(500)]
        public string Comentario { get; set; } = null!;

        [Column("fecha_valoracion")]
        public DateTime FechaValoracion { get; set; } = DateTime.Now;

        // Navegaciones
        [ForeignKey("IdNotificacion")]
        public virtual Notificacion? IdNotificacionNavigation { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuarios? IdUsuarioNavigation { get; set; }
    }
}