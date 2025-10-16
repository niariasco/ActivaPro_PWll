using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class Roles
{
    [Key]
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = null!;
    public string? Descripcion { get; set; }

    public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
