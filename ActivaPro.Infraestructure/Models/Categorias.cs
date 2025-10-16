using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

[Table("Categorias")]
public class Categorias
{
    [Key] 
    public int id_categoria { get; set; }
    public string nombre_categoria { get; set; }

    // Relaciones
    public virtual ICollection<Etiquetas> CategoriaEtiquetas { get; set; } = new List<Etiquetas>();
    public virtual ICollection<Especialidades> CategoriaEspecialidades { get; set; } = new List<Especialidades>();

    // Relación con SLA_Tickets
    public virtual ICollection<SLA_Tickets> SLA_Tickets { get; set; } = new List<SLA_Tickets>();
}