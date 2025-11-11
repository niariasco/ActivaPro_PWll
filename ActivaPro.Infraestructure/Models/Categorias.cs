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
    public string nombre_categoria { get; set; } = string.Empty;

    // n-n
    public virtual ICollection<Categoria_Etiqueta> CategoriaEtiquetas { get; set; } = new List<Categoria_Etiqueta>();
    public virtual ICollection<Categoria_Especialidad> CategoriaEspecialidades { get; set; } = new List<Categoria_Especialidad>();
    public virtual ICollection<Categoria_SLA> CategoriaSLAs { get; set; } = new List<Categoria_SLA>();
}