using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.DTOs
{
    public class RegisterDTO
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Contrasena { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public int NumeroSucursal { get; set; } = 0;
    }


    public class ProfileDTO
    {
        public int IdUsuario { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public int NumeroSucursal { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;
    }

    public class ChangePasswordDTO
    {
        [Required]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}