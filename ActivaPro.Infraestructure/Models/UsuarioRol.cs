using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class UsuarioRol
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public virtual Roles IdRolNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
