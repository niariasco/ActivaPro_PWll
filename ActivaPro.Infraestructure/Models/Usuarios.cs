using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class Usuarios
{
    [Key]
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public int NumeroSucursal { get; set; }
    public string Correo { get; set; } = null!;
    public string Contrasena { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<Usuario_Especialidad> UsuarioEspecialidades { get; set; } = new List<Usuario_Especialidad>();
}
