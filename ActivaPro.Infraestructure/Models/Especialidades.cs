using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models;

[Table("Especialidades")]
public class Especialidades
{
    [Key]
    [Column("id_especialidad")]
    public int id_especialidad { get; set; }

    [Required]
    [Column("nombre_especialidad")]
    [MaxLength(100)]
    public string NombreEspecialidad { get; set; } = string.Empty;

}
