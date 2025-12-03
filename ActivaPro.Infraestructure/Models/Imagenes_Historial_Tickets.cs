using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Imagenes_Historial_Tickets")]
    public class Imagenes_Historial_Tickets
    {
        [Key]
        [Column("id_imagen_historial")]
        public int IdImagenHistorial { get; set; }

        [Column("id_historial")]
        public int IdHistorial { get; set; }

        [Column("nombre_archivo")]
        public string NombreArchivo { get; set; } = null!;

        [Column("ruta_archivo")]
        public string RutaArchivo { get; set; } = null!;

        [Column("fecha_subida")]
        public DateTime FechaSubida { get; set; }

        [ForeignKey("IdHistorial")]
        public virtual Historial_Tickets Historial { get; set; } = null!;
    }
}