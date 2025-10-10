using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Especialidades
{
    public int IdEspecialidad { get; set; }

    public string NombreEspecialidad { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Usuarios> IdUsuario { get; set; } = new List<Usuarios>();
}
