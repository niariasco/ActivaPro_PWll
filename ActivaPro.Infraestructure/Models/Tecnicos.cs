using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Tecnicos
{
    public int IdTecnico { get; set; }

    public int IdUsuario { get; set; }

    public int CargaTrabajo { get; set; }

    public bool Disponible { get; set; }

    public string? Especialidades { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
