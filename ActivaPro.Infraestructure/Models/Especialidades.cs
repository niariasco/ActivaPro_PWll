using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models;

public partial class Especialidades
{
    [Key]
    public int id_especialidad { get; set; }
    public string NombreEspecialidad { get; set; }

    [Column("id_categoria")]
    public int? id_categoria { get; set; }

    [ForeignKey("id_categoria")]
    public Categorias Categoria { get; set; }
}
