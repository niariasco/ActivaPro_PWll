using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Categorias
{
    [Key] 
    public int id_categoria { get; set; }
    public string nombre_categoria { get; set; } = string.Empty;

    // n-n
    public virtual ICollection<Categoria_Etiqueta> CategoriaEtiquetas { get; set; } = new List<Categoria_Etiqueta>();
    public virtual ICollection<Categoria_Especialidad> CategoriaEspecialidades { get; set; } = new List<Categoria_Especialidad>();
    public virtual ICollection<Categoria_SLA> CategoriaSLAs { get; set; } = new List<Categoria_SLA>();
}