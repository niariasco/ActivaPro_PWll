using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Application.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        public bool Recordarme { get; set; }
    }
}