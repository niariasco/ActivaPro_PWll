using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models;

public partial class Usuarios
{
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [Column("nombre")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Column("numero_sucursal")]
    public int NumeroSucursal { get; set; }

    [Required]
    [Column("correo")]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [Column("contrasena")]
    [MaxLength(255)]
    public string Contrasena { get; set; } = string.Empty; // HASH PBKDF2

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("ultimo_inicio_sesion")]
    public DateTime? UltimoInicioSesion { get; set; }

    public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<Usuario_Especialidad> UsuarioEspecialidades { get; set; } = new List<Usuario_Especialidad>();
}
