using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Etiquetas")]
    public class Etiquetas
    {
        [Key]
        [Column("id_etiqueta")]
        public int id_etiqueta { get; set; }

        [Required]
        [Column("nombre_etiqueta")]
        [MaxLength(100)]
        public string nombre_etiqueta { get; set; } = string.Empty;


    }
}
