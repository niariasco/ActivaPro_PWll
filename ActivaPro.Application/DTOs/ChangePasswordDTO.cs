using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.DTOs
{

        public class ChangePasswordDTO
        {
            public int IdUsuario { get; set; }

            [Required(ErrorMessage = "Contraseña actual es obligatoria")]
            [DataType(DataType.Password)]
            public string ContrasenaActual { get; set; } = string.Empty;

            [Required(ErrorMessage = "Nueva contraseña es obligatoria")]
            [DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
            public string NuevaContrasena { get; set; } = string.Empty;
        }
    }

