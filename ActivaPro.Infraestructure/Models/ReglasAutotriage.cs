using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class ReglasAutotriage
{
    [Key]
    public int IdRegla { get; set; }

    public string NombreRegla { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string Condicion { get; set; } = null!;

    public int? AccionCategoria { get; set; }

    public string? AccionPrioridad { get; set; }

    public int? AccionUsuario { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Categorias? AccionCategoriaNavigation { get; set; }

    public virtual Usuarios? AccionUsuarioNavigation { get; set; }
}
