using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.DTOs
{
    public class TecnicosDTO : IValidatableObject
    {
        public int IdTecnico { get; set; }

        public int IdUsuario { get; set; }

        public int CargaTrabajo { get; set; } = 0;
        public bool Disponible { get; set; } = true;

        public List<int> EspecialidadesIds { get; set; } = new();

        // Para mostrar en vistas
        public List<string> EspecialidadesNombres { get; set; } = new();

        public string? NombreUsuario { get; set; }

        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string? CorreoUsuario { get; set; }

        // Validación condicional para Crear: si no viene IdUsuario, exigir Nombre/Correo
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IdUsuario <= 0)
            {
                if (string.IsNullOrWhiteSpace(NombreUsuario))
                {
                    yield return new ValidationResult("El nombre del usuario es obligatorio.", new[] { nameof(NombreUsuario) });
                }
                if (string.IsNullOrWhiteSpace(CorreoUsuario))
                {
                    yield return new ValidationResult("El correo del usuario es obligatorio.", new[] { nameof(CorreoUsuario) });
                }
            }
        }
    }
}
