using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class UsuarioRol
{
    [Key]
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public DateTime FechaAsignacion { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
    public virtual Roles Rol { get; set; } = null!;
}
