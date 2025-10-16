using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.DTOs
{
    public record TecnicosDTO
    {
        public int IdTecnico { get; set; }
        public int IdUsuario { get; set; }  // FK a Usuario
        public int CargaTrabajo { get; set; } = 0;
        public bool Disponible { get; set; } = true;
        public string? Especialidades { get; set; }

        // Información del Usuario relacionada
        public string? NombreUsuario { get; set; }
        public string? CorreoUsuario { get; set; }

    }
}
